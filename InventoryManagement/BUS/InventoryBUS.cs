
using System;
using System.Data;
using InventoryManagement.DAL;
using InventoryManagement.DTO;

namespace InventoryManagement.BUS
{
    public class InventoryBUS
    {
        private readonly VatTuDAL vatTuDAL = new();
        private readonly NhaCungCapDAL nhaCungCapDAL = new();
        private readonly KhoDAL khoDAL = new();

        public DataTable LayVatTu(string tuKhoa = "")
            => vatTuDAL.LayDanhSach(tuKhoa);

        public DataTable LayNhaCungCap(string tuKhoa = "")
            => nhaCungCapDAL.LayDanhSach(tuKhoa);

        public DataTable LayNhaCungCapCombo()
            => nhaCungCapDAL.LayDanhSachCombo();

        public DataTable LayLichSu()
            => khoDAL.LayLichSu();

        public DataTable LayCanhBao()
            => vatTuDAL.LayCanhBao();

        public decimal TinhTongGiaTriTonKho()
            => khoDAL.TinhTongGiaTriTonKho();

        public void ThemVatTu(VatTuDTO vt)
        {
            KiemTraVatTu(vt, true);
            vatTuDAL.Them(vt);
        }

        public void SuaVatTu(VatTuDTO vt)
        {
            KiemTraVatTu(vt, false);
            vatTuDAL.Sua(vt);
        }

        public void XoaVatTu(string ma)
        {
            if (string.IsNullOrWhiteSpace(ma))
                throw new Exception("Vui lòng chọn vật tư cần xóa.");

            vatTuDAL.Xoa(ma.Trim());
        }

        public void ThemNhaCungCap(NhaCungCapDTO ncc)
        {
            KiemTraNhaCungCap(ncc);
            nhaCungCapDAL.Them(ncc);
        }

        public void SuaNhaCungCap(NhaCungCapDTO ncc)
        {
            if (ncc.MaNCC <= 0)
                throw new Exception("Vui lòng chọn nhà cung cấp cần sửa.");

            KiemTraNhaCungCap(ncc);
            nhaCungCapDAL.Sua(ncc);
        }

        public void XoaNhaCungCap(int ma)
        {
            if (ma <= 0)
                throw new Exception("Vui lòng chọn nhà cung cấp cần xóa.");

            nhaCungCapDAL.Xoa(ma);
        }

        public int TaoPhieu(PhieuKhoDTO phieu)
        {
            if (phieu.ChiTiet == null || phieu.ChiTiet.Count == 0)
                throw new Exception("Vui lòng thêm ít nhất một vật tư vào phiếu.");

            return khoDAL.TaoPhieu(phieu);
        }

        private static void KiemTraVatTu(VatTuDTO vt, bool them)
        {
            if (string.IsNullOrWhiteSpace(vt.MaVatTu))
                throw new Exception("Mã vật tư không được để trống.");

            if (string.IsNullOrWhiteSpace(vt.TenVatTu))
                throw new Exception("Tên vật tư không được để trống.");

            if (vt.MaVatTu.Trim().Length > 20)
                throw new Exception("Mã vật tư tối đa 20 ký tự.");

            if (vt.SoLuongAnToan < 0)
                throw new Exception("Số lượng tồn an toàn không được âm.");

            if (them)
                vt.SoLuongTon = 0;

            vt.MaVatTu = vt.MaVatTu.Trim();
            vt.TenVatTu = vt.TenVatTu.Trim();
            vt.DVT = vt.DVT.Trim();
        }

        private static void KiemTraNhaCungCap(NhaCungCapDTO ncc)
        {
            if (string.IsNullOrWhiteSpace(ncc.TenNCC))
                throw new Exception("Tên nhà cung cấp không được để trống.");

            ncc.TenNCC = ncc.TenNCC.Trim();
            ncc.DienThoai = ncc.DienThoai.Trim();
            ncc.DiaChi = ncc.DiaChi.Trim();
        }
    }
}
