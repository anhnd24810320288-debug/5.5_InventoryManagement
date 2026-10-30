
using System;
using System.Data;
using MySql.Data.MySqlClient;
using InventoryManagement.DTO;

namespace InventoryManagement.DAL
{
    public class NhaCungCapDAL
    {
        public DataTable LayDanhSach(string tuKhoa = "")
        {
            using var conn = Database.GetConnection();
            conn.Open();

            string sql = @"
                SELECT MaNCC, TenNCC, DienThoai, DiaChi
                FROM NhaCungCap
                WHERE TenNCC LIKE @tk OR DienThoai LIKE @tk
                ORDER BY MaNCC";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tk", "%" + tuKhoa + "%");

            using var adapter = new MySqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public DataTable LayDanhSachCombo()
        {
            using var conn = Database.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(
                "SELECT MaNCC, TenNCC FROM NhaCungCap ORDER BY TenNCC", conn);
            using var adapter = new MySqlDataAdapter(cmd);

            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public void Them(NhaCungCapDTO ncc)
        {
            using var conn = Database.GetConnection();
            conn.Open();

            string sql = @"
                INSERT INTO NhaCungCap (TenNCC, DienThoai, DiaChi)
                VALUES (@ten, @dt, @dc)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ten", ncc.TenNCC);
            cmd.Parameters.AddWithValue("@dt",
                string.IsNullOrWhiteSpace(ncc.DienThoai)
                    ? DBNull.Value : ncc.DienThoai);
            cmd.Parameters.AddWithValue("@dc", ncc.DiaChi);
            ThucThi(cmd);
        }

        public void Sua(NhaCungCapDTO ncc)
        {
            using var conn = Database.GetConnection();
            conn.Open();

            string sql = @"
                UPDATE NhaCungCap
                SET TenNCC = @ten, DienThoai = @dt, DiaChi = @dc
                WHERE MaNCC = @ma";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ma", ncc.MaNCC);
            cmd.Parameters.AddWithValue("@ten", ncc.TenNCC);
            cmd.Parameters.AddWithValue("@dt",
                string.IsNullOrWhiteSpace(ncc.DienThoai)
                    ? DBNull.Value : ncc.DienThoai);
            cmd.Parameters.AddWithValue("@dc", ncc.DiaChi);
            ThucThi(cmd);
        }

        public void Xoa(int maNCC)
        {
            using var conn = Database.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(
                "DELETE FROM NhaCungCap WHERE MaNCC = @ma", conn);
            cmd.Parameters.AddWithValue("@ma", maNCC);

            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Không tìm thấy nhà cung cấp.");
            }
            catch (MySqlException ex) when (ex.Number == 1451)
            {
                throw new Exception(
                    "Không thể xóa nhà cung cấp đã được sử dụng trong phiếu kho.");
            }
        }

        private static void ThucThi(MySqlCommand cmd)
        {
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Không có dữ liệu nào được cập nhật.");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                throw new Exception("Số điện thoại nhà cung cấp đã tồn tại.");
            }
        }
    }
}
