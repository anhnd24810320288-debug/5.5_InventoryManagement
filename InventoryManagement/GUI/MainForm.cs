
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using InventoryManagement.BUS;
using InventoryManagement.DTO;

namespace InventoryManagement.GUI
{
    public class MainForm : Form
    {
        private readonly InventoryBUS bus = new();

        private readonly DataGridView dgvVatTu = new();
        private readonly DataGridView dgvNCC = new();
        private readonly DataGridView dgvChiTiet = new();
        private readonly DataGridView dgvLichSu = new();
        private readonly DataGridView dgvCanhBao = new();

        private readonly TextBox txtMaVT = new();
        private readonly TextBox txtTenVT = new();
        private readonly TextBox txtDVT = new();
        private readonly TextBox txtTonAnToan = new();
        private readonly TextBox txtTimVatTu = new();

        private readonly TextBox txtTenNCC = new();
        private readonly TextBox txtDienThoai = new();
        private readonly TextBox txtDiaChi = new();
        private readonly TextBox txtTimNCC = new();

        private readonly ComboBox cboLoaiPhieu = new();
        private readonly ComboBox cboNCC = new();
        private readonly ComboBox cboVatTu = new();
        private readonly TextBox txtSoLuong = new();
        private readonly TextBox txtDonGia = new();
        private readonly Label lblTongGiaTri = new();

        private readonly DataTable dtChiTiet = new();

        public MainForm()
        {
            Text = "HỆ THỐNG QUẢN LÝ XUẤT - NHẬP KHO NGUYÊN VẬT LIỆU";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1250, 780);
            MinimumSize = new Size(1000, 650);
            Font = new Font("Segoe UI", 10);

            BuildUI();
            NapDuLieu();
        }

        private void BuildUI()
        {
            var tabs = new TabControl { Dock = DockStyle.Fill };

            tabs.TabPages.Add(TaoTabVatTu());
            tabs.TabPages.Add(TaoTabNCC());
            tabs.TabPages.Add(TaoTabPhieu());
            tabs.TabPages.Add(TaoTabLichSu());

            Controls.Add(tabs);
        }

        private static Button TaoNut(string text)
        {
            return new Button
            {
                Text = text,
                AutoSize = true,
                Height = 34,
                Margin = new Padding(4)
            };
        }

        private static void CauHinhGrid(DataGridView grid)
        {
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.BackgroundColor = Color.White;
        }

        private static TableLayoutPanel TaoKhung(
            int soDong, int dongCoDinh, int dongCoDinhThuHai = 0)
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = soDong,
                Padding = new Padding(8)
            };

            if (soDong == 3)
            {
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, dongCoDinh));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, dongCoDinhThuHai));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            }

            return root;
        }

        private TabPage TaoTabVatTu()
        {
            var tab = new TabPage("Quản lý vật tư");
            var root = TaoKhung(3, 48, 175);

            var tim = new FlowLayoutPanel { Dock = DockStyle.Fill };
            tim.Controls.Add(new Label
            {
                Text = "Tìm mã hoặc tên vật tư:",
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0)
            });

            txtTimVatTu.Width = 230;
            tim.Controls.Add(txtTimVatTu);

            var btnTim = TaoNut("Tìm kiếm");
            btnTim.Click += (_, _) => Chay(NapVatTu);
            tim.Controls.Add(btnTim);

            var nhap = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            ThemTruong(nhap, "Mã vật tư", txtMaVT);
            ThemTruong(nhap, "Tên vật tư", txtTenVT);
            ThemTruong(nhap, "Đơn vị tính", txtDVT);
            ThemTruong(nhap, "Ngưỡng tồn an toàn", txtTonAnToan);

            var btnThem = TaoNut("Thêm vật tư");
            btnThem.Click += (_, _) => Chay(() =>
            {
                bus.ThemVatTu(new VatTuDTO
                {
                    MaVatTu = txtMaVT.Text,
                    TenVatTu = txtTenVT.Text,
                    DVT = txtDVT.Text,
                    SoLuongAnToan = DocSo(txtTonAnToan.Text, "Ngưỡng tồn an toàn")
                });

                NapVatTu();
                NapComboVatTu();
                XoaNhapVatTu();
                ThongBao("Thêm vật tư thành công.");
            });
            nhap.Controls.Add(btnThem);

            var btnSua = TaoNut("Cập nhật");
            btnSua.Click += (_, _) => Chay(() =>
            {
                bus.SuaVatTu(new VatTuDTO
                {
                    MaVatTu = txtMaVT.Text,
                    TenVatTu = txtTenVT.Text,
                    DVT = txtDVT.Text,
                    SoLuongAnToan = DocSo(txtTonAnToan.Text, "Ngưỡng tồn an toàn")
                });

                NapVatTu();
                NapCanhBao();
                NapComboVatTu();
                ThongBao("Cập nhật vật tư thành công.");
            });
            nhap.Controls.Add(btnSua);

            var btnXoa = TaoNut("Xóa vật tư");
            btnXoa.Click += (_, _) =>
            {
                if (MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa vật tư đã chọn?",
                    "Xác nhận xóa",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                Chay(() =>
                {
                    bus.XoaVatTu(txtMaVT.Text);
                    NapVatTu();
                    NapCanhBao();
                    NapComboVatTu();
                    XoaNhapVatTu();
                    ThongBao("Xóa vật tư thành công.");
                });
            };
            nhap.Controls.Add(btnXoa);

            var btnMoi = TaoNut("Làm mới");
            btnMoi.Click += (_, _) => XoaNhapVatTu();
            nhap.Controls.Add(btnMoi);

            CauHinhGrid(dgvVatTu);
            dgvVatTu.CellClick += (_, _) =>
            {
                if (dgvVatTu.CurrentRow == null) return;

                txtMaVT.Text = GiaTri(dgvVatTu, "MaVatTu");
                txtTenVT.Text = GiaTri(dgvVatTu, "TenVatTu");
                txtDVT.Text = GiaTri(dgvVatTu, "DVT");
                txtTonAnToan.Text = GiaTri(dgvVatTu, "SoLuongAnToan");
                txtMaVT.Enabled = false;
            };

            root.Controls.Add(tim, 0, 0);
            root.Controls.Add(nhap, 0, 1);
            root.Controls.Add(dgvVatTu, 0, 2);
            tab.Controls.Add(root);
            return tab;
        }

        private TabPage TaoTabNCC()
        {
            var tab = new TabPage("Quản lý nhà cung cấp");
            var root = TaoKhung(3, 48, 125);

            var tim = new FlowLayoutPanel { Dock = DockStyle.Fill };
            tim.Controls.Add(new Label
            {
                Text = "Tìm tên hoặc số điện thoại:",
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0)
            });

            txtTimNCC.Width = 230;
            tim.Controls.Add(txtTimNCC);

            var btnTim = TaoNut("Tìm kiếm");
            btnTim.Click += (_, _) => Chay(NapNCC);
            tim.Controls.Add(btnTim);

            var nhap = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            ThemTruong(nhap, "Tên nhà cung cấp", txtTenNCC);
            ThemTruong(nhap, "Số điện thoại", txtDienThoai);
            ThemTruong(nhap, "Địa chỉ", txtDiaChi);

            var btnThem = TaoNut("Thêm nhà cung cấp");
            btnThem.Click += (_, _) => Chay(() =>
            {
                bus.ThemNhaCungCap(new NhaCungCapDTO
                {
                    TenNCC = txtTenNCC.Text,
                    DienThoai = txtDienThoai.Text,
                    DiaChi = txtDiaChi.Text
                });

                NapNCC();
                NapComboNCC();
                XoaNhapNCC();
                ThongBao("Thêm nhà cung cấp thành công.");
            });
            nhap.Controls.Add(btnThem);

            var btnSua = TaoNut("Cập nhật");
            btnSua.Click += (_, _) => Chay(() =>
            {
                if (dgvNCC.CurrentRow == null)
                    throw new Exception("Vui lòng chọn nhà cung cấp cần sửa.");

                bus.SuaNhaCungCap(new NhaCungCapDTO
                {
                    MaNCC = int.Parse(GiaTri(dgvNCC, "MaNCC")),
                    TenNCC = txtTenNCC.Text,
                    DienThoai = txtDienThoai.Text,
                    DiaChi = txtDiaChi.Text
                });

                NapNCC();
                NapComboNCC();
                ThongBao("Cập nhật nhà cung cấp thành công.");
            });
            nhap.Controls.Add(btnSua);

            var btnXoa = TaoNut("Xóa nhà cung cấp");
            btnXoa.Click += (_, _) =>
            {
                if (dgvNCC.CurrentRow == null)
                {
                    ThongBao("Vui lòng chọn nhà cung cấp cần xóa.");
                    return;
                }

                if (MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa nhà cung cấp này?",
                    "Xác nhận xóa",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                Chay(() =>
                {
                    bus.XoaNhaCungCap(
                        int.Parse(GiaTri(dgvNCC, "MaNCC")));

                    NapNCC();
                    NapComboNCC();
                    XoaNhapNCC();
                    ThongBao("Xóa nhà cung cấp thành công.");
                });
            };
            nhap.Controls.Add(btnXoa);

            var btnMoi = TaoNut("Làm mới");
            btnMoi.Click += (_, _) => XoaNhapNCC();
            nhap.Controls.Add(btnMoi);

            CauHinhGrid(dgvNCC);
            dgvNCC.CellClick += (_, _) =>
            {
                if (dgvNCC.CurrentRow == null) return;

                txtTenNCC.Text = GiaTri(dgvNCC, "TenNCC");
                txtDienThoai.Text = GiaTri(dgvNCC, "DienThoai");
                txtDiaChi.Text = GiaTri(dgvNCC, "DiaChi");
            };

            root.Controls.Add(tim, 0, 0);
            root.Controls.Add(nhap, 0, 1);
            root.Controls.Add(dgvNCC, 0, 2);
            tab.Controls.Add(root);
            return tab;
        }

        private TabPage TaoTabPhieu()
        {
            var tab = new TabPage("Lập phiếu xuất - nhập");
            var root = TaoKhung(3, 50, 150);

            var loai = new FlowLayoutPanel { Dock = DockStyle.Fill };

            loai.Controls.Add(new Label
            {
                Text = "Loại phiếu:",
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0)
            });

            cboLoaiPhieu.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLoaiPhieu.Items.AddRange(new object[]
            {
                "NHAP - Phiếu nhập",
                "XUAT - Phiếu xuất"
            });
            cboLoaiPhieu.SelectedIndex = 0;
            cboLoaiPhieu.Width = 190;
            loai.Controls.Add(cboLoaiPhieu);

            loai.Controls.Add(new Label
            {
                Text = "Nhà cung cấp:",
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0)
            });

            cboNCC.DropDownStyle = ComboBoxStyle.DropDownList;
            cboNCC.Width = 260;
            loai.Controls.Add(cboNCC);

            var nhap = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            nhap.Controls.Add(new Label
            {
                Text = "Vật tư:",
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0)
            });

            cboVatTu.DropDownStyle = ComboBoxStyle.DropDownList;
            cboVatTu.Width = 240;
            nhap.Controls.Add(cboVatTu);

            ThemTruong(nhap, "Số lượng", txtSoLuong);
            ThemTruong(nhap, "Đơn giá", txtDonGia);

            var btnThemDong = TaoNut("Thêm vật tư");
            btnThemDong.Click += (_, _) => ThemDongPhieu();
            nhap.Controls.Add(btnThemDong);

            var btnXoaDong = TaoNut("Xóa dòng");
            btnXoaDong.Click += (_, _) =>
            {
                if (dgvChiTiet.CurrentRow != null &&
                    !dgvChiTiet.CurrentRow.IsNewRow)
                {
                    dgvChiTiet.Rows.Remove(dgvChiTiet.CurrentRow);
                    CapNhatTongTien();
                }
            };
            nhap.Controls.Add(btnXoaDong);

            var btnLuu = TaoNut("Lưu phiếu");
            btnLuu.Click += (_, _) => LuuPhieu();
            nhap.Controls.Add(btnLuu);

            lblTongGiaTri.AutoSize = true;
            lblTongGiaTri.Padding = new Padding(8);
            lblTongGiaTri.Text = "Tổng tiền: 0";
            nhap.Controls.Add(lblTongGiaTri);

            dtChiTiet.Columns.Add("MaVatTu", typeof(string));
            dtChiTiet.Columns.Add("TenVatTu", typeof(string));
            dtChiTiet.Columns.Add("SoLuong", typeof(decimal));
            dtChiTiet.Columns.Add("DonGia", typeof(decimal));
            dtChiTiet.Columns.Add("ThanhTien", typeof(decimal));

            CauHinhGrid(dgvChiTiet);
            dgvChiTiet.DataSource = dtChiTiet;

            root.Controls.Add(loai, 0, 0);
            root.Controls.Add(nhap, 0, 1);
            root.Controls.Add(dgvChiTiet, 0, 2);
            tab.Controls.Add(root);
            return tab;
        }

        private TabPage TaoTabLichSu()
        {
            var tab = new TabPage("Lịch sử và cảnh báo");
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(8)
            };

            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            var thanhCongCu = new FlowLayoutPanel { Dock = DockStyle.Fill };

            var btnTaiLai = TaoNut("Tải lại dữ liệu");
            btnTaiLai.Click += (_, _) => Chay(NapDuLieu);
            thanhCongCu.Controls.Add(btnTaiLai);

            var btnTinh = TaoNut("Tính giá trị tồn kho");
            btnTinh.Click += (_, _) => Chay(() =>
            {
                decimal tong = bus.TinhTongGiaTriTonKho();
                MessageBox.Show(
                    "Tổng giá trị tồn kho ước tính: " + tong.ToString("N2") + " VNĐ",
                    "Giá trị tồn kho",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            });
            thanhCongCu.Controls.Add(btnTinh);

            CauHinhGrid(dgvLichSu);
            CauHinhGrid(dgvCanhBao);

            var nhomLichSu = new GroupBox
            {
                Text = "Lịch sử phiếu kho",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            nhomLichSu.Controls.Add(dgvLichSu);

            var nhomCanhBao = new GroupBox
            {
                Text = "Vật tư có số lượng tồn thấp",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            nhomCanhBao.Controls.Add(dgvCanhBao);

            root.Controls.Add(thanhCongCu, 0, 0);
            root.Controls.Add(nhomLichSu, 0, 1);
            root.Controls.Add(nhomCanhBao, 0, 2);
            tab.Controls.Add(root);
            return tab;
        }

        private static void ThemTruong(
    FlowLayoutPanel panel, string ten, TextBox txt)
        {
            bool laNguongTonAnToan = ten == "Ngưỡng tồn an toàn";

            var nhom = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Width = laNguongTonAnToan ? 225 : 170,
                Height = 65,
                Margin = new Padding(4, 3, 10, 3)
            };

            nhom.Controls.Add(new Label
            {
                Text = ten,
                AutoSize = true
            });

            txt.Width = laNguongTonAnToan ? 205 : 155;
            nhom.Controls.Add(txt);

            panel.Controls.Add(nhom);
        }

        private static decimal DocSo(string text, string tenTruong)
        {
            if (!decimal.TryParse(text, out decimal value))
                throw new Exception(tenTruong + " phải là số hợp lệ.");

            return value;
        }

        private static string GiaTri(DataGridView grid, string tenCot)
        {
            if (grid.CurrentRow == null ||
                !grid.Columns.Contains(tenCot))
                return "";

            return Convert.ToString(
                grid.CurrentRow.Cells[tenCot].Value) ?? "";
        }

        private static void ThongBao(string noiDung)
        {
            MessageBox.Show(noiDung, "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void Chay(Action hanhDong)
        {
            try
            {
                hanhDong();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Có lỗi xảy ra",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void NapDuLieu()
        {
            NapVatTu();
            NapNCC();
            NapComboNCC();
            NapComboVatTu();
            NapLichSu();
            NapCanhBao();
        }

        private void NapVatTu()
        {
            dgvVatTu.DataSource = bus.LayVatTu(txtTimVatTu.Text.Trim());
        }

        private void NapNCC()
        {
            dgvNCC.DataSource = bus.LayNhaCungCap(txtTimNCC.Text.Trim());
        }

        private void NapComboNCC()
        {
            cboNCC.DataSource = bus.LayNhaCungCapCombo();
            cboNCC.DisplayMember = "TenNCC";
            cboNCC.ValueMember = "MaNCC";
        }

        private void NapComboVatTu()
        {
            cboVatTu.DataSource = bus.LayVatTu();
            cboVatTu.DisplayMember = "TenVatTu";
            cboVatTu.ValueMember = "MaVatTu";
        }

        private void NapLichSu()
        {
            dgvLichSu.DataSource = bus.LayLichSu();

            if (dgvLichSu.Columns.Contains("LoaiPhieu"))
            {
                dgvLichSu.Columns["LoaiPhieu"].HeaderText = "Loại phiếu";
                foreach (DataGridViewRow row in dgvLichSu.Rows)
                {
                    if (Convert.ToString(row.Cells["LoaiPhieu"].Value) == "NHAP")
                        row.Cells["LoaiPhieu"].Value = "Nhập kho";
                    else
                        row.Cells["LoaiPhieu"].Value = "Xuất kho";
                }
            }

            if (dgvLichSu.Columns.Contains("MaPhieu"))
                dgvLichSu.Columns["MaPhieu"].HeaderText = "Mã phiếu";
            if (dgvLichSu.Columns.Contains("NgayLap"))
                dgvLichSu.Columns["NgayLap"].HeaderText = "Ngày lập";
            if (dgvLichSu.Columns.Contains("TenNCC"))
                dgvLichSu.Columns["TenNCC"].HeaderText = "Nhà cung cấp";
            if (dgvLichSu.Columns.Contains("TongTien"))
                dgvLichSu.Columns["TongTien"].HeaderText = "Tổng tiền";
        }

        private void NapCanhBao()
        {
            dgvCanhBao.DataSource = bus.LayCanhBao();

            if (dgvCanhBao.Columns.Contains("MaVatTu"))
                dgvCanhBao.Columns["MaVatTu"].HeaderText = "Mã vật tư";
            if (dgvCanhBao.Columns.Contains("TenVatTu"))
                dgvCanhBao.Columns["TenVatTu"].HeaderText = "Tên vật tư";
            if (dgvCanhBao.Columns.Contains("DVT"))
                dgvCanhBao.Columns["DVT"].HeaderText = "Đơn vị tính";
            if (dgvCanhBao.Columns.Contains("SoLuongTon"))
                dgvCanhBao.Columns["SoLuongTon"].HeaderText = "Số lượng tồn";
            if (dgvCanhBao.Columns.Contains("SoLuongAnToan"))
                dgvCanhBao.Columns["SoLuongAnToan"].HeaderText = "Ngưỡng tồn an toàn";
        }

        private void XoaNhapVatTu()
        {
            txtMaVT.Clear();
            txtTenVT.Clear();
            txtDVT.Clear();
            txtTonAnToan.Text = "10";
            txtMaVT.Enabled = true;
        }

        private void XoaNhapNCC()
        {
            txtTenNCC.Clear();
            txtDienThoai.Clear();
            txtDiaChi.Clear();
        }

        private void ThemDongPhieu()
        {
            try
            {
                if (cboVatTu.SelectedValue == null)
                    throw new Exception("Vui lòng chọn vật tư.");

                string ma = Convert.ToString(cboVatTu.SelectedValue) ?? "";
                string ten = cboVatTu.Text;
                decimal sl = DocSo(txtSoLuong.Text, "Số lượng");
                decimal gia = DocSo(txtDonGia.Text, "Đơn giá");

                if (sl <= 0)
                    throw new Exception("Số lượng phải lớn hơn 0.");

                if (gia < 0)
                    throw new Exception("Đơn giá không được âm.");

                foreach (DataRow row in dtChiTiet.Rows)
                {
                    if (Convert.ToString(row["MaVatTu"]) == ma)
                        throw new Exception(
                            "Vật tư này đã có trong phiếu. Vui lòng không thêm trùng.");
                }

                dtChiTiet.Rows.Add(ma, ten, sl, gia, sl * gia);
                CapNhatTongTien();
                txtSoLuong.Clear();
                txtDonGia.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CapNhatTongTien()
        {
            decimal tong = 0;

            foreach (DataRow row in dtChiTiet.Rows)
                tong += Convert.ToDecimal(row["ThanhTien"]);

            lblTongGiaTri.Text = "Tổng tiền: " + tong.ToString("N2") + " VNĐ";
        }

        private void LuuPhieu()
        {
            Chay(() =>
            {
                if (dtChiTiet.Rows.Count == 0)
                    throw new Exception("Vui lòng thêm ít nhất một vật tư.");

                string loai = cboLoaiPhieu.SelectedIndex == 0 ? "NHAP" : "XUAT";

                int? maNCC = null;
                if (loai == "NHAP" && cboNCC.SelectedValue != null)
                    maNCC = Convert.ToInt32(cboNCC.SelectedValue);

                var phieu = new PhieuKhoDTO
                {
                    LoaiPhieu = loai,
                    MaNCC = maNCC
                };

                foreach (DataRow row in dtChiTiet.Rows)
                {
                    phieu.ChiTiet.Add(new ChiTietPhieuKhoDTO
                    {
                        MaVatTu = Convert.ToString(row["MaVatTu"]) ?? "",
                        SoLuong = Convert.ToDecimal(row["SoLuong"]),
                        DonGia = Convert.ToDecimal(row["DonGia"])
                    });
                }

                int maPhieu = bus.TaoPhieu(phieu);

                dtChiTiet.Rows.Clear();
                CapNhatTongTien();
                NapDuLieu();

                MessageBox.Show(
                    "Lập phiếu thành công. Mã phiếu: " + maPhieu,
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            });
        }
    }
}
