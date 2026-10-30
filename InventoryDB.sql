
CREATE DATABASE IF NOT EXISTS InventoryDB
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE InventoryDB;

CREATE TABLE IF NOT EXISTS NhaCungCap (
    MaNCC INT AUTO_INCREMENT PRIMARY KEY,
    TenNCC VARCHAR(150) NOT NULL,
    DienThoai VARCHAR(15) UNIQUE,
    DiaChi VARCHAR(255)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS VatTu (
    MaVatTu VARCHAR(20) PRIMARY KEY,
    TenVatTu VARCHAR(150) NOT NULL,
    DVT VARCHAR(20),
    SoLuongTon DECIMAL(18,2) NOT NULL DEFAULT 0,
    CHECK (SoLuongTon >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS PhieuKho (
    MaPhieu INT AUTO_INCREMENT PRIMARY KEY,
    LoaiPhieu VARCHAR(10) NOT NULL,
    NgayLap DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    MaNCC INT NULL,
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0,
    CHECK (LoaiPhieu IN ('NHAP', 'XUAT')),
    CONSTRAINT FK_PhieuKho_NCC
        FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ChiTietPhieuKho (
    MaPhieu INT NOT NULL,
    MaVatTu VARCHAR(20) NOT NULL,
    SoLuong DECIMAL(18,2) NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL DEFAULT 0,
    PRIMARY KEY (MaPhieu, MaVatTu),
    CHECK (SoLuong > 0),
    CHECK (DonGia >= 0),
    CONSTRAINT FK_ChiTiet_Phieu
        FOREIGN KEY (MaPhieu) REFERENCES PhieuKho(MaPhieu),
    CONSTRAINT FK_ChiTiet_VatTu
        FOREIGN KEY (MaVatTu) REFERENCES VatTu(MaVatTu)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS NguongTonKho (
    MaVatTu VARCHAR(20) PRIMARY KEY,
    SoLuongAnToan DECIMAL(18,2) NOT NULL DEFAULT 10,
    CHECK (SoLuongAnToan >= 0),
    CONSTRAINT FK_Nguong_VatTu
        FOREIGN KEY (MaVatTu) REFERENCES VatTu(MaVatTu)
        ON DELETE CASCADE
) ENGINE=InnoDB;

INSERT IGNORE INTO NhaCungCap
    (MaNCC, TenNCC, DienThoai, DiaChi)
VALUES
    (1, 'Công ty Vật tư An Phát', '0901234567', 'Hà Nội'),
    (2, 'Công ty Nguyên liệu Minh Đức', '0912345678', 'Bắc Ninh');

INSERT IGNORE INTO VatTu
    (MaVatTu, TenVatTu, DVT, SoLuongTon)
VALUES
    ('VT001', 'Thép xây dựng', 'Kg', 100),
    ('VT002', 'Xi măng', 'Bao', 50),
    ('VT003', 'Ốc vít', 'Cái', 200);

INSERT IGNORE INTO NguongTonKho
    (MaVatTu, SoLuongAnToan)
VALUES
    ('VT001', 20),
    ('VT002', 20),
    ('VT003', 20);

SELECT * FROM NhaCungCap;
SELECT * FROM VatTu;
SELECT * FROM NguongTonKho;
