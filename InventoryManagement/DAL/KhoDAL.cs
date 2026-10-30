
using System;
using System.Data;
using MySql.Data.MySqlClient;
using InventoryManagement.DTO;

namespace InventoryManagement.DAL
{
    public class KhoDAL
    {
        public int TaoPhieu(PhieuKhoDTO phieu)
        {
            if (phieu.ChiTiet == null || phieu.ChiTiet.Count == 0)
                throw new Exception("Phiếu kho phải có ít nhất một vật tư.");

            if (phieu.LoaiPhieu != "NHAP" && phieu.LoaiPhieu != "XUAT")
                throw new Exception("Loại phiếu không hợp lệ.");

            using var conn = Database.GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                decimal tongTien = 0;

                foreach (var ct in phieu.ChiTiet)
                {
                    if (ct.SoLuong <= 0)
                        throw new Exception("Số lượng phải lớn hơn 0.");

                    if (ct.DonGia < 0)
                        throw new Exception("Đơn giá không được âm.");

                    tongTien += ct.SoLuong * ct.DonGia;
                }

                int maPhieu;

                using (var cmd = new MySqlCommand(
                    @"INSERT INTO PhieuKho (LoaiPhieu, MaNCC, TongTien)
                      VALUES (@loai, @ncc, @tong)", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@loai", phieu.LoaiPhieu);
                    cmd.Parameters.AddWithValue("@ncc",
                        phieu.MaNCC.HasValue
                            ? phieu.MaNCC.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@tong", tongTien);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new MySqlCommand(
                    "SELECT LAST_INSERT_ID()", conn, tran))
                {
                    maPhieu = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach (var ct in phieu.ChiTiet)
                {
                    decimal tonHienTai;

                    using (var cmd = new MySqlCommand(
                        @"SELECT SoLuongTon FROM VatTu
                          WHERE MaVatTu = @ma FOR UPDATE", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@ma", ct.MaVatTu);
                        object? result = cmd.ExecuteScalar();

                        if (result == null || result == DBNull.Value)
                            throw new Exception(
                                "Không tìm thấy vật tư: " + ct.MaVatTu);

                        tonHienTai = Convert.ToDecimal(result);
                    }

                    if (phieu.LoaiPhieu == "XUAT" &&
                        tonHienTai < ct.SoLuong)
                    {
                        throw new Exception("Không đủ hàng trong kho");
                    }

                    using (var cmd = new MySqlCommand(
                        @"INSERT INTO ChiTietPhieuKho
                          (MaPhieu, MaVatTu, SoLuong, DonGia)
                          VALUES (@phieu, @ma, @sl, @gia)", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@phieu", maPhieu);
                        cmd.Parameters.AddWithValue("@ma", ct.MaVatTu);
                        cmd.Parameters.AddWithValue("@sl", ct.SoLuong);
                        cmd.Parameters.AddWithValue("@gia", ct.DonGia);
                        cmd.ExecuteNonQuery();
                    }

                    string sqlCapNhat = phieu.LoaiPhieu == "NHAP"
                        ? @"UPDATE VatTu
                            SET SoLuongTon = SoLuongTon + @sl
                            WHERE MaVatTu = @ma"
                        : @"UPDATE VatTu
                            SET SoLuongTon = SoLuongTon - @sl
                            WHERE MaVatTu = @ma AND SoLuongTon >= @sl";

                    using (var cmd = new MySqlCommand(
                        sqlCapNhat, conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@sl", ct.SoLuong);
                        cmd.Parameters.AddWithValue("@ma", ct.MaVatTu);

                        if (cmd.ExecuteNonQuery() == 0)
                            throw new Exception("Không đủ hàng trong kho");
                    }
                }

                tran.Commit();
                return maPhieu;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public DataTable LayLichSu()
        {
            using var conn = Database.GetConnection();
            conn.Open();

            string sql = @"
                SELECT p.MaPhieu, p.LoaiPhieu, p.NgayLap,
                       n.TenNCC, p.TongTien
                FROM PhieuKho p
                LEFT JOIN NhaCungCap n ON p.MaNCC = n.MaNCC
                ORDER BY p.MaPhieu DESC";

            using var cmd = new MySqlCommand(sql, conn);
            using var adapter = new MySqlDataAdapter(cmd);

            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public decimal TinhTongGiaTriTonKho()
        {
            using var conn = Database.GetConnection();
            conn.Open();

            string sql = @"
                SELECT COALESCE(SUM(
                    v.SoLuongTon * COALESCE(
                        (
                            SELECT ct.DonGia
                            FROM ChiTietPhieuKho ct
                            INNER JOIN PhieuKho p
                                ON p.MaPhieu = ct.MaPhieu
                            WHERE ct.MaVatTu = v.MaVatTu
                              AND p.LoaiPhieu = 'NHAP'
                            ORDER BY p.NgayLap DESC, p.MaPhieu DESC
                            LIMIT 1
                        ), 0
                    )
                ), 0)
                FROM VatTu v";

            using var cmd = new MySqlCommand(sql, conn);
            object? result = cmd.ExecuteScalar();
            return result == null || result == DBNull.Value
                ? 0 : Convert.ToDecimal(result);
        }
    }
}
