
using System.Collections.Generic;

namespace InventoryManagement.DTO
{
    public class ChiTietPhieuKhoDTO
    {
        public string MaVatTu { get; set; } = "";
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }

    public class PhieuKhoDTO
    {
        public string LoaiPhieu { get; set; } = "NHAP";
        public int? MaNCC { get; set; }
        public List<ChiTietPhieuKhoDTO> ChiTiet { get; set; } = new();
    }
}
