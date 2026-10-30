
using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using InventoryManagement.DTO;

namespace InventoryManagement.DAL
{
    public class VatTuDAL
    {
        public DataTable LayDanhSach(string tuKhoa = "")
        {
            using var conn = Database.GetConnection();
            conn.Open();

            string sql = @"
                SELECT v.MaVatTu, v.TenVatTu, v.DVT, v.SoLuongTon,
                       COALESCE(n.SoLuongAnToan, 10) AS SoLuongAnToan
                FROM VatTu v
                LEFT JOIN NguongTonKho n ON v.MaVatTu = n.MaVatTu
                WHERE v.MaVatTu LIKE @tk OR v.TenVatTu LIKE @tk
                ORDER BY v.MaVatTu";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tk", "%" + tuKhoa + "%");

            using var adapter = new MySqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public void Them(VatTuDTO vt)
        {
            using var conn = Database.GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                using (var cmd = new MySqlCommand(
                    @"INSERT INTO VatTu (MaVatTu, TenVatTu, DVT, SoLuongTon)
                      VALUES (@ma, @ten, @dvt, 0)", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@ma", vt.MaVatTu);
                    cmd.Parameters.AddWithValue("@ten", vt.TenVatTu);
                    cmd.Parameters.AddWithValue("@dvt", vt.DVT);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new MySqlCommand(
                    @"INSERT INTO NguongTonKho (MaVatTu, SoLuongAnToan)
                      VALUES (@ma, @nguong)", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@ma", vt.MaVatTu);
                    cmd.Parameters.AddWithValue("@nguong", vt.SoLuongAnToan);
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void Sua(VatTuDTO vt)
        {
            using var conn = Database.GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                using (var cmd = new MySqlCommand(
                    @"UPDATE VatTu
                      SET TenVatTu = @ten, DVT = @dvt
                      WHERE MaVatTu = @ma", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@ma", vt.MaVatTu);
                    cmd.Parameters.AddWithValue("@ten", vt.TenVatTu);
                    cmd.Parameters.AddWithValue("@dvt", vt.DVT);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new Exception("Không tìm thấy vật tư cần sửa.");
                }

                using (var cmd = new MySqlCommand(
                    @"INSERT INTO NguongTonKho (MaVatTu, SoLuongAnToan)
                      VALUES (@ma, @nguong)
                      ON DUPLICATE KEY UPDATE SoLuongAnToan = @nguong", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@ma", vt.MaVatTu);
                    cmd.Parameters.AddWithValue("@nguong", vt.SoLuongAnToan);
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void Xoa(string maVatTu)
        {
            using var conn = Database.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(
                "DELETE FROM VatTu WHERE MaVatTu = @ma", conn);
            cmd.Parameters.AddWithValue("@ma", maVatTu);

            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Không tìm thấy vật tư.");
            }
            catch (MySqlException ex) when (ex.Number == 1451)
            {
                throw new Exception(
                    "Không thể xóa vật tư đã phát sinh phiếu kho.");
            }
        }

        public DataTable LayCanhBao()
        {
            using var conn = Database.GetConnection();
            conn.Open();

            string sql = @"
                SELECT v.MaVatTu, v.TenVatTu, v.DVT, v.SoLuongTon,
                       COALESCE(n.SoLuongAnToan, 10) AS SoLuongAnToan
                FROM VatTu v
                LEFT JOIN NguongTonKho n ON v.MaVatTu = n.MaVatTu
                WHERE v.SoLuongTon <= COALESCE(n.SoLuongAnToan, 10)
                ORDER BY v.SoLuongTon ASC";

            using var cmd = new MySqlCommand(sql, conn);
            using var adapter = new MySqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }
    }
}
