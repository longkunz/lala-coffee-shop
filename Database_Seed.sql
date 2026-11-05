-- =============================================
-- LalaCoffee Database Seed Script (Completely Fresh)
-- Target Database: UniCafe (.NET 4.8 LalaCoffee)
-- Encoding: UTF-8 with N'...' for perfect Vietnamese
-- =============================================

USE UniCafe;
GO

SET NOCOUNT ON;

-- 1. Clean existing data in reverse order of foreign keys
DELETE FROM OrderDetails;
DELETE FROM Orders;
DELETE FROM OptionProducts;
DELETE FROM PropertyProducts;
DELETE FROM Products;
DELETE FROM Categories;
DELETE FROM Blogs;
DELETE FROM AspNetUserRoles;
DELETE FROM AspNetUserClaims;
DELETE FROM AspNetUserLogins;
DELETE FROM AspNetUsers;
DELETE FROM AspNetRoles;

DBCC CHECKIDENT ('Categories', RESEED, 0);
DBCC CHECKIDENT ('Products', RESEED, 0);
DBCC CHECKIDENT ('OptionProducts', RESEED, 0);
DBCC CHECKIDENT ('PropertyProducts', RESEED, 0);
DBCC CHECKIDENT ('Orders', RESEED, 0);
DBCC CHECKIDENT ('OrderDetails', RESEED, 0);
DBCC CHECKIDENT ('Blogs', RESEED, 0);

-- =============================================
-- 2. Seed AspNetRoles
-- =============================================
INSERT INTO AspNetRoles (Id, Name, Discriminator)
VALUES 
    ('11111111-1111-1111-1111-111111111111', N'Admin', N'IdentityRole'),
    ('22222222-2222-2222-2222-222222222222', N'User', N'IdentityRole');

-- =============================================
-- 3. Seed AspNetUsers (Admin & Customer)
-- Password for Admin: Admin@123
-- Password for User:  User@123
-- =============================================
INSERT INTO AspNetUsers (
    Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, 
    PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, 
    LockoutEndDateUtc, LockoutEnabled, AccessFailedCount, UserName
)
VALUES 
(
    '99999999-9999-9999-9999-999999999999',
    N'admin@lalacoffee.vn', 1,
    N'AKdgfQL6E5xmRrV/3I8IUVGpEgMyhTUtbypGQsTiIcFfoI8vhVoNc46w4BZ7kU+M0A==',
    N'a1b2c3d4-e5f6-7890-1234-56789abcdef0',
    N'0901234567', 1, 0, NULL, 0, 0,
    N'admin'
),
(
    '88888888-8888-8888-8888-888888888888',
    N'user@lalacoffee.vn', 1,
    N'AO/jeKDdiNQSL6rU8PMhalHFDHrIdy/LGeSRp32h9Hy/PMPwMmUb8/K8PohCFb62MA==',
    N'b2c3d4e5-f6a7-8901-2345-6789abcdef01',
    N'0912345678', 1, 0, NULL, 0, 0,
    N'user'
);

-- Assign Roles
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES 
    ('99999999-9999-9999-9999-999999999999', '11111111-1111-1111-1111-111111111111'),
    ('88888888-8888-8888-8888-888888888888', '22222222-2222-2222-2222-222222222222');

-- =============================================
-- 4. Seed Categories
-- =============================================
SET IDENTITY_INSERT Categories ON;

INSERT INTO Categories (Id, Name, Slug, Description, ParentId, Status, CreatedAt, UpdatedAt)
VALUES
(1, N'Cà Phê Truyền Thống', N'ca-phe-truyen-thong', N'Hương vị Robusta Đắk Lắk đậm đà nguyên bản, nét văn hóa cà phê Việt Nam.', 0, N'1', GETDATE(), GETDATE()),
(2, N'Cà Phê Pha Máy', N'ca-phe-pha-may', N'Espresso, Latte, Caramel Macchiato chuẩn vị phong cách Ý từ hạt Arabica Cầu Đất.', 0, N'1', GETDATE(), GETDATE()),
(3, N'Cold Brew Đặc Sản', N'cold-brew', N'Cà phê ủ lạnh 16 tiếng thanh mát, hương hoa quả tự nhiên.', 0, N'1', GETDATE(), GETDATE()),
(4, N'Trà Trái Cây & Macchiato', N'tra-trai-cay', N'Trà hoa quả tươi mát kết hợp lớp bọt kem cheese béo mịn ngất ngây.', 0, N'1', GETDATE(), GETDATE()),
(5, N'Trà Sữa & Đá Xay', N'tra-sua-da-xay', N'Trà sữa Oolong nướng thơm ngậy cùng các món đá xay mát lạnh giải nhiệt.', 0, N'1', GETDATE(), GETDATE()),
(6, N'Bánh Mì & Tráng Miệng', N'banh-mi-trang-mieng', N'Bánh mì Việt Nam giòn rụm, bánh ngọt croissant và mochi kem tuyệt hảo.', 0, N'1', GETDATE(), GETDATE());

SET IDENTITY_INSERT Categories OFF;

-- =============================================
-- 5. Seed Products
-- =============================================
SET IDENTITY_INSERT Products ON;

INSERT INTO Products (Id, Name, Slug, Price, Description, Image, Status, Show, Category_Id, CreatedAt, UpdatedAt)
VALUES
(1, N'Cà Phê Đen Đá', N'ca-phe-den-da', 29000, N'Cà phê đen đá đậm đà từ hạt Robusta Buôn Ma Thuột hảo hạng, hậu vị ngọt thanh.', N'/Content/Layout/User/images/ca-phe-den-da.jpg', N'1', 1, 1, GETDATE(), GETDATE()),
(2, N'Cà Phê Sữa Đá', N'ca-phe-sua-da', 35000, N'Cà phê sữa đá ngọt béo hài hòa, hương vị quốc hồn quốc túy của người Việt.', N'/Content/Layout/User/images/ca-phe-sua-da.jpg', N'1', 1, 1, GETDATE(), GETDATE()),
(3, N'Bạc Xỉu Lala', N'bac-xiu', 39000, N'Nhiều sữa ít cà phê, béo ngậy vị sữa đặc và thơm nồng cà phê dịu nhẹ.', N'/Content/Layout/User/images/bac-xiu.jpg', N'1', 1, 1, GETDATE(), GETDATE()),

(4, N'Latte Đá', N'latte-da', 49000, N'Espresso pha cùng sữa tươi thanh trùng mịn màng, thơm ngậy nhẹ nhàng.', N'/Content/images/latte-da.jpg', N'1', 1, 2, GETDATE(), GETDATE()),
(5, N'Caramel Macchiato', N'caramel-macchiato', 55000, N'Sự kết hợp quyến rũ giữa sữa tươi, sốt vani thơm lừng và caramel ngọt ngào.', N'/Content/images/caramel-macchiato.jpg', N'1', 1, 2, GETDATE(), GETDATE()),

(6, N'Cold Brew Sữa Tươi', N'cold-brew-sua-tuoi', 45000, N'Cà phê ủ lạnh hòa quyện cùng sữa tươi tạo vị béo thanh mát và sảng khoái.', N'/Content/images/cold-brew-sua-tuoi.jpg', N'1', 1, 3, GETDATE(), GETDATE()),

(7, N'Trà Đào Cam Sả', N'tra-dao-cam-sa', 45000, N'Trà đào thanh ngọt kết hợp vị cam vàng chua nhẹ và hương sả thơm nồng.', N'/Content/images/tra-dao-cam-sa.jpg', N'1', 1, 4, GETDATE(), GETDATE()),
(8, N'Trà Vải Đá Tuyết Yuzu', N'tra-vai-da-tuyet-yuzu', 49000, N'Trà vải thanh tao kết hợp cùng đá tuyết yuzu Nhật Bản mát lạnh độc đáo.', N'/Content/images/tra-vai-da-tuyet-yuzu.jpg', N'1', 1, 4, GETDATE(), GETDATE()),
(9, N'Trà Đen Macchiato', N'tra-den-macchiato', 42000, N'Cốt trà đen đậm vị phủ lớp kem sữa phô mai macchiato béo mặn đặc trưng.', N'/Content/images/tra-den-matchiato.jpg', N'1', 1, 4, GETDATE(), GETDATE()),

(10, N'Trà Sữa Oolong Nướng Trân Châu', N'tra-sua-oolong-nuong-tran-chau', 49000, N'Hương vị trà Oolong nướng đậm đà, sữa thơm ngậy cùng trân châu dai giòn.', N'/Content/images/tra-sua-oolong-nuong-tran-chau.jpg', N'1', 1, 5, GETDATE(), GETDATE()),
(11, N'Chocolate Đá', N'chocolate-da', 45000, N'Cacao nguyên chất pha sữa tươi thơm lừng, đậm vị ngọt ngào ấm áp.', N'/Content/images/chocolate-da.jpg', N'1', 1, 5, GETDATE(), GETDATE()),
(12, N'Đào Việt Quất Đá Xay', N'dao-viet-quat-da-xay', 52000, N'Đá xay chua ngọt từ đào mọng nước và quả việt quất tươi ngon hảo hạng.', N'/Content/images/dao-viet-quat-da-xay.jpg', N'1', 1, 5, GETDATE(), GETDATE()),

(13, N'Bánh Mì Việt Nam Thịt Nguội', N'banh-mi-vietnam-thit-nguoi', 35000, N'Bánh mì giòn rụm kẹp pate gan béo ngậy, thịt nguội, chả lụa và dưa chua.', N'/Content/images/banh-mi-vietnam-thit-nguoi.jpg', N'1', 1, 6, GETDATE(), GETDATE()),
(14, N'Bánh Mì Gậy Cá Ngừ Mayo', N'banh-mi-gay-ca-ngu-mayo', 29000, N'Bánh mì que giòn tan nhân cá ngừ sốt mayonnaise béo thơm hấp dẫn.', N'/Content/images/banh-mi-gay-ca-ngu-mayo.jpg', N'1', 1, 6, GETDATE(), GETDATE()),
(15, N'Croissant Trứng Muối', N'croissant-trung-muoi', 39000, N'Bánh sừng bò ngàn lớp thơm lừng bơ Pháp, sốt trứng muối tan chảy đậm đà.', N'/Content/images/croissant-trung-muoi.jpg', N'1', 1, 6, GETDATE(), GETDATE()),
(16, N'Mochi Kem Matcha', N'mochi-kem-matcha', 22000, N'Bánh mochi dẻo mềm nhân kem trà xanh Nhật Bản mát lạnh thanh tao.', N'/Content/images/mochi-kem-matcha.jpg', N'1', 1, 6, GETDATE(), GETDATE()),
(17, N'Mochi Kem Socola', N'mochi-kem-socola', 22000, N'Bánh mochi bọc kem socola ngọt ngào, mát lạnh béo ngậy khó cưỡng.', N'/Content/images/mochi-kem-socola.jpg', N'1', 1, 6, GETDATE(), GETDATE());

SET IDENTITY_INSERT Products OFF;

-- =============================================
-- 6. Seed PropertyProducts (Thuộc tính: Đá / Đường)
-- =============================================
SET IDENTITY_INSERT PropertyProducts ON;

INSERT INTO PropertyProducts (Id, Name, Price, Status, Product_Id, CreatedAt, UpdatedAt)
VALUES
(1, N'100% Đá (Bình thường)', 0, N'1', 1, GETDATE(), GETDATE()),
(2, N'50% Đá (Ít đá)', 0, N'1', 1, GETDATE(), GETDATE()),
(3, N'Không đá', 0, N'1', 1, GETDATE(), GETDATE()),
(4, N'100% Đá (Bình thường)', 0, N'1', 2, GETDATE(), GETDATE()),
(5, N'50% Đá (Ít đá)', 0, N'1', 2, GETDATE(), GETDATE()),
(6, N'Không đá', 0, N'1', 2, GETDATE(), GETDATE()),
(7, N'100% Đá (Bình thường)', 0, N'1', 3, GETDATE(), GETDATE()),
(8, N'50% Đá (Ít đá)', 0, N'1', 3, GETDATE(), GETDATE()),
(9, N'Không đá', 0, N'1', 3, GETDATE(), GETDATE()),
(10, N'100% Đá (Bình thường)', 0, N'1', 7, GETDATE(), GETDATE()),
(11, N'50% Đá (Ít đá)', 0, N'1', 7, GETDATE(), GETDATE()),
(12, N'100% Đá (Bình thường)', 0, N'1', 10, GETDATE(), GETDATE()),
(13, N'50% Đá (Ít đá)', 0, N'1', 10, GETDATE(), GETDATE()),
(14, N'Bình thường', 0, N'1', 13, GETDATE(), GETDATE()),
(15, N'Nướng nóng giòn', 0, N'1', 13, GETDATE(), GETDATE()),
(16, N'Nướng nóng giòn', 0, N'1', 15, GETDATE(), GETDATE());

SET IDENTITY_INSERT PropertyProducts OFF;

-- =============================================
-- 7. Seed OptionProducts (Tùy chọn: Size / Topping)
-- =============================================
SET IDENTITY_INSERT OptionProducts ON;

INSERT INTO OptionProducts (Id, Name, Price, Status, Product_Id, CreatedAt, UpdatedAt)
VALUES
(1, N'Size Vừa (M)', 0, N'1', 1, GETDATE(), GETDATE()),
(2, N'Size Lớn (L)', 6000, N'1', 1, GETDATE(), GETDATE()),
(3, N'Size Vừa (M)', 0, N'1', 2, GETDATE(), GETDATE()),
(4, N'Size Lớn (L)', 6000, N'1', 2, GETDATE(), GETDATE()),
(5, N'Size Vừa (M)', 0, N'1', 3, GETDATE(), GETDATE()),
(6, N'Size Lớn (L)', 6000, N'1', 3, GETDATE(), GETDATE()),
(7, N'Thêm Kem Cheese Macchiato', 12000, N'1', 7, GETDATE(), GETDATE()),
(8, N'Thêm Thạch Đào Giòn', 10000, N'1', 7, GETDATE(), GETDATE()),
(9, N'Thêm Trân Châu Trắng', 10000, N'1', 10, GETDATE(), GETDATE()),
(10, N'Thêm Pudding Trứng', 10000, N'1', 10, GETDATE(), GETDATE()),
(11, N'Thêm Kem Cheese', 12000, N'1', 10, GETDATE(), GETDATE()),
(12, N'Thêm Sốt Phô Mai', 8000, N'1', 13, GETDATE(), GETDATE());

SET IDENTITY_INSERT OptionProducts OFF;

-- =============================================
-- 8. Seed Blogs
-- =============================================
SET IDENTITY_INSERT Blogs ON;

INSERT INTO Blogs (Id, Title, Content, CreatedDate, Published, Author, Image)
VALUES
(
    1, 
    N'Câu chuyện hạt cà phê LalaCoffee - Hương vị từ đất đỏ Tây Nguyên',
    N'<p>Tại <strong>LalaCoffee</strong>, mỗi hạt cà phê đều được chọn lọc khắt khe từ vùng đất trứ danh Đắk Lắk và Lâm Đồng. Chúng tôi tự hào mang đến cho khách hàng những ly cà phê nguyên bản, thơm ngon đậm đà và giàu bản sắc Việt Nam.</p>',
    GETDATE(),
    1,
    N'LalaCoffee Team',
    N'/Content/Layout/User/images/blog1.jpg'
),
(
    2, 
    N'Nghệ thuật pha chế Cold Brew: Tinh tế và đậm đà trong từng giọt',
    N'<p>Cold Brew là phương pháp ủ cà phê bằng nước lạnh suốt 16-24 tiếng đồng hồ. Từng giọt cà phê chiết xuất mang hương hoa quả thanh dịu, hậu ngọt tự nhiên và hoàn toàn êm ái cho dạ dày của bạn.</p>',
    GETDATE(),
    1,
    N'Barista Lala',
    N'/Content/Layout/User/images/blog2.jpg'
),
(
    3, 
    N'Không gian hẹn hò & làm việc đầy cảm hứng tại LalaCoffee',
    N'<p>Không chỉ phục vụ đồ uống chất lượng, <strong>LalaCoffee</strong> còn là điểm hẹn lý tưởng với không gian ấm cúng, âm nhạc nhẹ nhàng và bàn ghế tiện nghi cho cả làm việc nhóm lẫn thư giãn cá nhân.</p>',
    GETDATE(),
    1,
    N'LalaCoffee Team',
    N'/Content/Layout/User/images/blog3.jpg'
);

SET IDENTITY_INSERT Blogs OFF;
