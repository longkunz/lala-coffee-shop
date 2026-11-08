-- LalaCoffee | SQL Server | UTF-8
-- Chạy trong database đã có schema. Script cập nhật bộ seed, không xóa dữ liệu.
-- Tài khoản mẫu CHỈ tạo khi chưa có: admin / Admin@123, user / User@123.
-- Đổi mật khẩu mẫu trước khi mở web công khai.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
-- Nội dung seed được cập nhật theo Slug (menu), Title (blog), Product_Id + Name (tùy chọn).
-- Giữ nguyên ID, tài khoản, mật khẩu, đơn hàng và dữ liệu ngoài bộ seed.
DECLARE @Now datetime = GETDATE();
DECLARE @Categories TABLE (Name nvarchar(max), Slug nvarchar(200) PRIMARY KEY, Description nvarchar(max));
INSERT INTO @Categories VALUES
(N'Cà Phê Truyền Thống',N'ca-phe-truyen-thong',N'Một góc quen thuộc dành cho người yêu hương cà phê Việt: đậm vị, thơm nồng và có hậu vị lưu lại sau mỗi ngụm. Từ đen đá mộc mạc đến sữa đá hài hòa hay bạc xỉu dịu ngọt, mỗi lựa chọn mang một sắc thái riêng để bạn tìm được ly cà phê hợp gu. Thưởng thức cùng bánh mì vào buổi sáng hoặc nhâm nhi chậm rãi trong lúc trò chuyện để cảm nhận trọn vẹn sự gần gũi của những thức uống quen tên.'),
(N'Cà Phê Pha Máy',N'ca-phe-pha-may',N'Khám phá sự cân bằng giữa hương cà phê và độ mềm mại của sữa qua những thức uống pha máy quen thuộc. Latte mang đến cảm giác nhẹ nhàng, trong khi Caramel Macchiato nổi bật với hương caramel và vị ngọt tròn đầy. Đây là nhóm đồ uống dành cho những buổi sáng cần khởi đầu êm dịu hoặc giờ nghỉ muốn đổi vị. Bạn có thể chọn kích cỡ phù hợp và thêm một shot espresso nếu muốn hương cà phê rõ nét hơn.'),
(N'Cold Brew Đặc Sản',N'cold-brew',N'Dành cho những ngày muốn thưởng thức cà phê theo một nhịp khác, Cold Brew kết hợp hương cà phê ủ lạnh với cảm giác mát dịu khi uống cùng đá. Phiên bản sữa tươi giúp vị cà phê trở nên mềm mại, dễ làm quen mà vẫn giữ được nét riêng trong hậu vị. Hãy nhấp từng ngụm nhỏ để cảm nhận sự thay đổi khi cà phê hòa dần vào sữa. Một lựa chọn phù hợp cho giờ nghỉ trưa hoặc buổi chiều cần chút hương vị thư thả.'),
(N'Trà Trái Cây & Macchiato',N'tra-trai-cay',N'Những nền trà thơm kết hợp cùng đào, vải, cam sả hay phúc bồn tử tạo nên bảng hương vị phong phú, từ thanh nhẹ đến chua ngọt nổi bật. Nếu thích sự béo mềm, bạn có thể thử trà đen macchiato hoặc thêm lớp kem cheese cho món trà yêu thích. Nhóm đồ uống này phù hợp cho một buổi gặp gỡ bạn bè, giờ nghỉ giữa ngày hoặc khi muốn đổi gu sau những ly cà phê quen thuộc. Chọn theo loại trái cây bạn yêu thích để bắt đầu hành trình khám phá.'),
(N'Trà Sữa & Đá Xay',N'tra-sua-da-xay',N'Một bộ sưu tập dành cho những ai thích vị trà quyện sữa, kết cấu sánh mịn và cảm giác mát lạnh. Oolong nướng, hồng trà, mắc ca, chocolate cùng các món đá xay mang đến nhiều cách thưởng thức khác nhau trong cùng một nhóm đồ uống. Trân châu, pudding và kem cheese là những lựa chọn thêm để bạn điều chỉnh trải nghiệm theo sở thích. Từ ly trà sữa quen thuộc đến món đá xay để nhâm nhi, luôn có một gợi ý cho buổi chiều muốn tự thưởng.'),
(N'Bánh Mì & Tráng Miệng',N'banh-mi-trang-mieng',N'Hoàn thiện buổi hẹn cà phê bằng một phần bánh mặn hoặc món tráng miệng nhỏ xinh. Bánh mì thịt nguội, bánh mì gậy và croissant phù hợp khi bạn muốn ăn nhẹ, còn mochi kem đem lại điểm kết mát dịu với nhiều hương vị để lựa chọn. Bạn có thể kết hợp bánh mặn với cà phê sữa hoặc dùng mochi cùng một ly trà. Hãy xem mô tả từng món để chọn nhân bánh, cách phục vụ và hương vị phù hợp với khẩu vị của mình.');

IF EXISTS (SELECT c.Slug FROM dbo.Categories c JOIN @Categories s ON s.Slug=c.Slug GROUP BY c.Slug HAVING COUNT(*)>1)
    THROW 51001, N'Danh mục trùng slug trong bộ seed. Hãy xử lý dữ liệu trùng trước.', 1;
UPDATE c SET Name=s.Name, Description=s.Description, UpdatedAt=@Now
FROM dbo.Categories c JOIN @Categories s ON c.Slug=s.Slug;
INSERT dbo.Categories (Name,Slug,Description,ParentId,Status,CreatedAt,UpdatedAt)
SELECT s.Name,s.Slug,s.Description,0,N'1',@Now,@Now FROM @Categories s
WHERE NOT EXISTS (SELECT 1 FROM dbo.Categories c WHERE c.Slug=s.Slug);

DECLARE @Products TABLE (Name nvarchar(max), Slug nvarchar(200) PRIMARY KEY, Price decimal(18,2), Description nvarchar(max), Image nvarchar(max), CategorySlug nvarchar(200));
INSERT INTO @Products VALUES
(N'Cà Phê Đen Đá',N'ca-phe-den-da',29000,N'Cà phê đen đá dành cho người yêu vị cà phê rõ nét, ít bị che lấp bởi sữa hay các lớp kem. Hương rang thơm mở đầu cho vị đắng đặc trưng, sau đó để lại hậu vị lưu luyến khi thưởng thức chậm. Đá lạnh giúp ly cà phê có cảm giác gọn và sảng khoái, phù hợp cho một buổi sáng bắt đầu công việc hoặc lúc cần nghỉ ngơi giữa ngày. Bạn có thể dùng kèm bánh mì thịt nguội để cân bằng vị giác. Nếu muốn tăng độ đậm, hãy chọn thêm shot espresso trong phần tùy chọn.',N'/Content/Layout/User/images/ca-phe-den-da.jpg',N'ca-phe-truyen-thong'),
(N'Cà Phê Sữa Đá',N'ca-phe-sua-da',35000,N'Một ly cà phê sữa đá quen thuộc với sự hòa quyện giữa hương cà phê rang và độ ngọt béo của sữa đặc. Khi khuấy đều, từng ngụm mang lại cảm giác đậm vừa, dễ uống và có hậu vị mềm hơn cà phê đen. Món này phù hợp với người thích hương cà phê truyền thống nhưng vẫn muốn chút ngọt ngào trong buổi sáng. Thưởng thức ngay khi nhận để cảm nhận rõ sự cân bằng trước khi đá tan nhiều. Gợi ý dùng cùng bánh mì hoặc croissant trứng muối cho một bữa ăn nhẹ tròn vị.',N'/Content/Layout/User/images/ca-phe-sua-da.jpg',N'ca-phe-truyen-thong'),
(N'Bạc Xỉu Lala',N'bac-xiu',39000,N'Bạc Xỉu Lala đặt vị sữa ở trung tâm, điểm thêm hương cà phê để tạo cảm giác thơm nhẹ và gần gũi. Sữa đặc mang lại độ ngọt, sữa tươi giúp kết cấu mềm mại, còn lớp cà phê tạo điểm nhấn trong từng ngụm. Đây là lựa chọn dành cho người thích đồ uống thiên về sữa và không muốn vị đắng quá nổi bật. Khuấy đều trước khi uống để các lớp hương vị hòa vào nhau. Bạn có thể kết hợp với một phần bánh mặn để buổi sáng thêm phong phú. Món vẫn có chứa cà phê dù hương vị khá dịu.',N'/Content/Layout/User/images/bac-xiu.jpg',N'ca-phe-truyen-thong'),
(N'Latte Đá',N'latte-da',49000,N'Latte đá là cuộc gặp gỡ giữa espresso và sữa tươi, tạo nên một ly cà phê có kết cấu mềm và hương vị cân bằng. Vị cà phê hiện diện vừa đủ để làm nền cho độ béo nhẹ của sữa, phù hợp khi bạn muốn nhâm nhi lâu hơn trong một buổi đọc sách hoặc làm việc. Sau khi khuấy, espresso hòa đều vào sữa mang đến trải nghiệm nhất quán từ đầu đến cuối. Nếu thích cà phê rõ hơn, bạn có thể chọn thêm shot espresso. Dùng cùng croissant trứng muối là một gợi ý cho giờ nghỉ giữa ngày.',N'/Content/images/latte-da.jpg',N'ca-phe-pha-may'),
(N'Caramel Macchiato',N'caramel-macchiato',55000,N'Caramel Macchiato mang sắc thái ngọt ngào với hương caramel, chút vani và vị sữa mềm mại trên nền espresso. Khi các lớp hòa quyện, vị cà phê giúp cân bằng phần sốt ngọt, tạo hậu vị thơm và tròn đầy. Món phù hợp với người thích cà phê pha sữa có điểm nhấn rõ ràng, đặc biệt trong những buổi chiều muốn tự thưởng một thức uống dễ chịu. Bạn nên khuấy nhẹ để sốt caramel không đọng ở đáy ly. Có thể dùng kèm mochi kem socola nếu muốn một cặp hương vị thiên về món tráng miệng.',N'/Content/images/caramel-macchiato.jpg',N'ca-phe-pha-may'),
(N'Cold Brew Sữa Tươi',N'cold-brew-sua-tuoi',45000,N'Cold Brew Sữa Tươi kết hợp cà phê ủ lạnh với sữa để tạo cảm giác thanh và mềm khi uống cùng đá. Hương cà phê xuất hiện nhẹ nhàng ở đầu ngụm, tiếp nối bằng vị sữa và hậu vị dịu. Đây là một cách đổi gu cho người đã quen cà phê sữa đá và muốn thử cách thưởng thức khác. Hãy khuấy đều trước khi uống để lớp cà phê hòa vào sữa, sau đó nhâm nhi từng ngụm nhỏ. Món phù hợp cho giờ nghỉ trưa hoặc một buổi chiều thư thả, dùng riêng hay cùng bánh đều dễ kết hợp.',N'/Content/images/cold-brew-sua-tuoi.jpg',N'cold-brew'),
(N'Trà Đào Cam Sả',N'tra-dao-cam-sa',45000,N'Trà Đào Cam Sả kết hợp hương trà, vị đào ngọt dịu, chút chua của cam và mùi sả đặc trưng. Các thành phần tạo nên một ly trà có nhiều tầng hương nhưng vẫn dễ làm quen, phù hợp khi bạn muốn đổi vị sau cà phê. Khuấy nhẹ để phần trà và trái cây hòa đều, rồi thưởng thức cùng đá lạnh. Vị cam làm phần đào bớt ngọt gắt, trong khi sả để lại cảm giác thơm lâu hơn ở hậu vị. Bạn có thể thêm thạch đào hoặc trân châu trắng để tăng độ thú vị khi nhâm nhi.',N'/Content/images/tra-dao-cam-sa.jpg',N'tra-trai-cay'),
(N'Trà Vải Đá Tuyết Yuzu',N'tra-vai-da-tuyet-yuzu',49000,N'Trà Vải Đá Tuyết Yuzu mang đến sự kết hợp giữa vị vải thanh ngọt và hương cam quýt đặc trưng của yuzu. Phần đá tuyết tạo cảm giác mát nhẹ, giúp từng ngụm trà có kết cấu khác với trà trái cây thông thường. Khi thưởng thức, bạn có thể cảm nhận vị trái cây trước rồi đến hương trà ở phía sau. Món phù hợp cho người thích đồ uống thơm, có chút chua để cân bằng độ ngọt. Khuấy nhẹ khi đá tuyết bắt đầu tan và thưởng thức sớm để giữ được kết cấu đặc trưng của ly trà.',N'/Content/images/tra-vai-da-tuyet-yuzu.jpg',N'tra-trai-cay'),
(N'Trà Đen Macchiato',N'tra-den-macchiato',42000,N'Trà Đen Macchiato dành cho người thích sự tương phản giữa nền trà đậm và lớp kem sữa béo mềm. Hương trà đen giữ vai trò chủ đạo, còn macchiato mang đến chút vị mặn nhẹ để cân bằng độ ngọt. Bạn có thể nhấp qua lớp kem trước rồi uống cùng trà, hoặc khuấy đều để có một ly trà sữa mịn và đồng nhất hơn. Mỗi cách thưởng thức tạo ra một sắc thái riêng. Món hợp với những giờ nghỉ chậm rãi và có thể thêm trân châu trắng nếu bạn thích kết cấu dai nhẹ bên cạnh lớp kem.',N'/Content/images/tra-den-matchiato.jpg',N'tra-trai-cay'),
(N'Trà Sữa Oolong Nướng Trân Châu',N'tra-sua-oolong-nuong-tran-chau',49000,N'Trà Sữa Oolong Nướng Trân Châu nổi bật với hương trà rang ấm, hòa cùng vị sữa để tạo cảm giác đậm mà vẫn mềm mại. Trân châu là điểm nhấn về kết cấu, giúp mỗi ngụm có thêm độ dai vui miệng. Món phù hợp với người thích hương trà hiện diện rõ trong ly trà sữa, thay vì chỉ thiên về độ ngọt. Khuấy đều trước khi uống để trà và sữa hòa quyện. Bạn có thể thêm pudding trứng cho trải nghiệm béo mềm hơn hoặc dùng nguyên bản để cảm nhận rõ nét hương oolong nướng.',N'/Content/images/tra-sua-oolong-nuong-tran-chau.jpg',N'tra-sua-da-xay'),
(N'Chocolate Đá',N'chocolate-da',45000,N'Chocolate đá mang hương cacao quen thuộc kết hợp cùng sữa, tạo nên một thức uống ngọt dịu và có chiều sâu. Vị cacao hơi đắng giúp cân bằng độ béo, trong khi đá lạnh làm tổng thể dễ uống hơn. Đây là gợi ý cho người muốn một món thiên về chocolate thay cho cà phê hoặc trà. Hãy khuấy đều để phần cacao không lắng ở đáy ly, rồi thưởng thức từng ngụm nhỏ. Nếu thích món tráng miệng đậm vị, bạn có thể dùng cùng mochi kem socola; còn một phần bánh mì sẽ giúp cân bằng cảm giác ngọt.',N'/Content/images/chocolate-da.jpg',N'tra-sua-da-xay'),
(N'Đào Việt Quất Đá Xay',N'dao-viet-quat-da-xay',52000,N'Đào Việt Quất Đá Xay là sự kết hợp giữa vị đào ngọt dịu và sắc thái chua nhẹ của việt quất trong một ly đá xay mát lạnh. Kết cấu sánh tạo cảm giác gần với món tráng miệng, thích hợp để nhâm nhi trong buổi gặp bạn bè hoặc giờ nghỉ cuối chiều. Hương trái cây thay đổi theo từng ngụm khi phần đá hòa dần vào đồ uống. Bạn nên thưởng thức sớm để cảm nhận rõ độ mịn của đá xay. Có thể dùng riêng hoặc kết hợp một phần bánh nhỏ nếu muốn có thêm món ăn nhẹ.',N'/Content/images/dao-viet-quat-da-xay.jpg',N'tra-sua-da-xay'),
(N'Bánh Mì Việt Nam Thịt Nguội',N'banh-mi-vietnam-thit-nguoi',35000,N'Bánh Mì Việt Nam Thịt Nguội là lựa chọn quen thuộc cho một bữa ăn gọn với phần vỏ giòn và nhân mặn hài hòa. Thịt nguội, chả lụa, pate cùng rau và đồ chua tạo nên nhiều lớp kết cấu trong mỗi miếng bánh. Phần nhân béo được cân bằng bởi vị chua nhẹ, giúp món dễ ăn hơn khi dùng cùng cà phê hoặc trà. Bạn có thể chọn nướng nóng giòn trong phần phục vụ để bánh thơm hơn khi nhận. Gợi ý kết hợp cùng cà phê sữa đá cho buổi sáng hoặc trà đào cam sả vào giờ trưa.',N'/Content/images/banh-mi-vietnam-thit-nguoi.jpg',N'banh-mi-trang-mieng'),
(N'Bánh Mì Gậy Cá Ngừ Mayo',N'banh-mi-gay-ca-ngu-mayo',29000,N'Bánh Mì Gậy Cá Ngừ Mayo có dáng bánh gọn, vỏ giòn và phần nhân cá ngừ trộn sốt mayonnaise béo mềm. Vị mặn nhẹ của cá ngừ kết hợp cùng sốt tạo nên món ăn vừa đủ cho một giờ nghỉ ngắn, không cần chuẩn bị cầu kỳ. Bạn có thể chọn nướng nóng để cảm nhận rõ sự tương phản giữa vỏ bánh và nhân. Nếu thích đậm vị hơn, phần tùy chọn có sốt phô mai để thêm vào. Món dễ kết hợp với trà trái cây hoặc latte đá khi muốn một phần ăn nhẹ cùng đồ uống.',N'/Content/images/banh-mi-gay-ca-ngu-mayo.jpg',N'banh-mi-trang-mieng'),
(N'Croissant Trứng Muối',N'croissant-trung-muoi',39000,N'Croissant Trứng Muối kết hợp lớp bánh xếp mềm bên trong, vỏ hơi giòn bên ngoài và phần sốt trứng muối đậm vị. Hương bơ của bánh làm nền cho độ béo và chút mặn của nhân, tạo một món ăn nhẹ có điểm nhấn. Thưởng thức khi bánh còn ấm sẽ cảm nhận rõ hơn hương thơm và kết cấu từng lớp. Bạn có thể chọn phục vụ nướng nóng trong phần tùy chọn. Một ly latte đá hoặc cà phê đen sẽ giúp cân bằng vị béo, phù hợp cho buổi sáng chậm rãi hay giờ nghỉ giữa chiều.',N'/Content/images/croissant-trung-muoi.jpg',N'banh-mi-trang-mieng'),
(N'Mochi Kem Matcha',N'mochi-kem-matcha',22000,N'Mochi Kem Matcha mang lớp vỏ nếp dẻo mềm ôm lấy phần kem trà xanh mát lạnh. Vị matcha hơi đắng nhẹ tạo điểm cân bằng cho độ ngọt của kem, trong khi lớp vỏ giúp mỗi miếng có thêm độ dai đặc trưng. Đây là món tráng miệng nhỏ để kết thúc một buổi cà phê hoặc dùng cùng bạn bè khi muốn thử nhiều hương vị. Nên thưởng thức ngay sau khi nhận để giữ được phần nhân kem. Nếu bánh còn cứng do bảo quản lạnh, chờ mềm nhẹ trước khi ăn để cảm nhận rõ kết cấu.',N'/Content/images/mochi-kem-matcha.jpg',N'banh-mi-trang-mieng'),
(N'Mochi Kem Socola',N'mochi-kem-socola',22000,N'Mochi Kem Socola là sự kết hợp giữa vỏ nếp dẻo và nhân kem chocolate thơm béo. Hương cacao tạo chiều sâu cho vị ngọt, còn lớp vỏ mềm giúp món bánh có cảm giác thú vị ngay từ miếng đầu tiên. Một phần nhỏ phù hợp để tự thưởng sau bữa ăn hoặc dùng kèm ly cà phê khi gặp bạn bè. Hãy thưởng thức sớm để phần kem giữ được độ mát và không chảy quá nhiều. Bạn có thể ghép cùng mochi matcha hoặc việt quất để so sánh hương vị và tạo một đĩa tráng miệng nhiều lựa chọn.',N'/Content/images/mochi-kem-socola.jpg',N'banh-mi-trang-mieng'),
(N'Hồng Trà Sữa Trân Châu',N'hong-tra-sua-tran-chau',45000,N'Hồng Trà Sữa Trân Châu mang hương trà đen rõ nét, kết hợp với vị sữa để tạo nên một lựa chọn quen thuộc và dễ thưởng thức. Trân châu bổ sung độ dai, giúp ly trà sữa có thêm nhịp điệu trong từng ngụm. Món phù hợp khi bạn muốn bắt đầu với một hương vị cơ bản trước khi thử những loại trà sữa khác. Khuấy đều để phần trà và sữa hòa quyện, sau đó thưởng thức cùng đá. Nếu thích độ béo nổi bật hơn, bạn có thể thêm pudding trứng hoặc kem cheese trong phần tùy chọn.',N'/Content/images/hong-tra-sua-tran-chau.jpg',N'tra-sua-da-xay'),
(N'Trà Sữa Mắc Ca',N'tra-sua-mac-ca',52000,N'Trà Sữa Mắc Ca kết hợp nền trà sữa với hương hạt thơm và cảm giác béo bùi đặc trưng. So với những ly trà trái cây thanh nhẹ, món này thiên về sự mềm mại và hậu vị tròn đầy, phù hợp cho một buổi chiều muốn đổi gu. Hương trà giúp cân bằng phần sữa để tổng thể không chỉ có vị ngọt. Bạn nên khuấy đều trước khi uống và có thể thêm trân châu trắng nếu thích kết cấu dai nhẹ. Món có thành phần hạt và sữa, vì vậy hãy cân nhắc nguyên liệu theo nhu cầu ăn uống của mình.',N'/Content/images/tra=sua-macca.jpg',N'tra-sua-da-xay'),
(N'Oolong Nướng Kem Dừa Đá Xay',N'oolong-nuong-kem-dua-da-xay',55000,N'Oolong Nướng Kem Dừa Đá Xay kết hợp hương trà rang với vị dừa béo thơm trong kết cấu đá xay sánh mịn. Hương oolong tạo chiều sâu, trong khi phần kem dừa làm mỗi ngụm mềm mại và có hậu vị kéo dài. Đây là gợi ý cho người thích món đồ uống gần với một phần tráng miệng, có thể nhâm nhi chậm trong lúc trò chuyện. Thưởng thức sớm để đá xay còn mịn và lớp kem giữ được độ bông. Bạn có thể khuấy nhẹ cho các thành phần hòa quyện hoặc thử từng lớp trước khi trộn đều.',N'/Content/images/olong-nuong-kem-dua-da-xay.jpg',N'tra-sua-da-xay'),
(N'Trà Phúc Bồn Tử',N'tra-phuc-bon-tu',49000,N'Trà Phúc Bồn Tử có vị chua ngọt nổi bật trên nền trà thơm, mang đến một cách đổi vị dễ chịu cho giờ nghỉ trong ngày. Hương quả mọng xuất hiện ở đầu ngụm rồi nhường chỗ cho hậu vị trà, tạo cảm giác cân bằng khi uống cùng đá. Món phù hợp với người yêu trà trái cây và muốn thử một lựa chọn khác đào hoặc vải. Hãy khuấy nhẹ để phần trái cây hòa đều trước khi thưởng thức. Có thể thêm trân châu trắng hoặc thạch đào nếu bạn muốn ly trà có thêm kết cấu để nhâm nhi.',N'/Content/Layout/User/images/tra-phuc-bon-tu.jpg',N'tra-trai-cay'),
(N'Bánh Mì Gậy Gà Kim Quất',N'banh-mi-gay-ga-kim-quat',32000,N'Bánh Mì Gậy Gà Kim Quất kết hợp vỏ bánh giòn với phần nhân gà và sốt có hương kim quất chua nhẹ. Hương cam quýt làm phần nhân mặn trở nên sinh động, tạo một lựa chọn khác bên cạnh bánh mì cá ngừ hoặc thịt nguội. Dáng bánh gọn phù hợp cho bữa ăn nhẹ giữa giờ hay dùng cùng đồ uống khi làm việc. Bạn có thể chọn nướng nóng để bánh thơm và giòn hơn lúc thưởng thức. Gợi ý dùng kèm trà đào cam sả hoặc cà phê sữa đá để có một cặp món hài hòa.',N'/Content/images/banh-mi-gay-ga-kim-quat.jpg',N'banh-mi-trang-mieng'),
(N'Mochi Kem Việt Quất',N'mochi-kem-viet-quat',22000,N'Mochi Kem Việt Quất mang lớp vỏ nếp dẻo ôm phần nhân kem có hương quả mọng và vị chua ngọt nhẹ. Sự kết hợp giữa độ dai của vỏ và cảm giác mát lạnh của kem tạo nên món tráng miệng nhỏ nhưng nhiều nét thú vị. Món hợp để chia sẻ cùng bạn bè khi muốn thử nhiều vị mochi trong một lần ghé quán. Thưởng thức sớm sau khi nhận để phần kem giữ được kết cấu. Bạn có thể dùng cùng trà đen macchiato hoặc một ly cà phê ít ngọt để cân bằng hương vị.',N'/Content/images/mochi-kem-viet-quat.jpg',N'banh-mi-trang-mieng'),
(N'Mochi Kem Phúc Bồn Tử',N'mochi-kem-phuc-bon-tu',22000,N'Mochi Kem Phúc Bồn Tử dành cho người thích món tráng miệng mát lạnh có điểm nhấn chua ngọt của quả mọng. Bên ngoài là lớp nếp mềm dẻo, bên trong là nhân kem thơm giúp mỗi miếng bánh có sự tương phản về kết cấu. Hương phúc bồn tử tạo cảm giác nhẹ nhàng hơn những món thiên về chocolate hoặc caramel. Nên thưởng thức ngay khi bánh vừa mềm nhẹ sau bảo quản lạnh để vỏ dẻo và nhân còn mát. Có thể ghép cùng mochi matcha hoặc socola để tạo một phần bánh nhiều sắc thái cho buổi hẹn cà phê.',N'/Content/images/mochi-kem-phuc-bon-tu.jpg',N'banh-mi-trang-mieng');

IF EXISTS (SELECT p.Slug FROM dbo.Products p JOIN @Products s ON s.Slug=p.Slug GROUP BY p.Slug HAVING COUNT(*)>1)
    THROW 51002, N'Sản phẩm trùng slug trong bộ seed. Hãy xử lý dữ liệu trùng trước.', 1;
UPDATE p SET Name=s.Name, Price=s.Price, Description=s.Description, Image=s.Image, Category_Id=c.Id, UpdatedAt=@Now
FROM dbo.Products p JOIN @Products s ON p.Slug=s.Slug JOIN dbo.Categories c ON c.Slug=s.CategorySlug;
INSERT dbo.Products (Name,Slug,Price,Description,Image,Status,Show,Category_Id,CreatedAt,UpdatedAt)
SELECT s.Name,s.Slug,s.Price,s.Description,s.Image,N'1',1,c.Id,@Now,@Now
FROM @Products s JOIN dbo.Categories c ON c.Slug=s.CategorySlug
WHERE NOT EXISTS (SELECT 1 FROM dbo.Products p WHERE p.Slug=s.Slug);

DECLARE @Properties TABLE (ProductSlug nvarchar(200), Name nvarchar(200), Price decimal(18,2), PRIMARY KEY(ProductSlug,Name));
INSERT INTO @Properties VALUES
(N'ca-phe-den-da',N'Size Vừa (M)',0),
(N'ca-phe-den-da',N'Size Lớn (L)',6000),
(N'ca-phe-sua-da',N'Size Vừa (M)',0),
(N'ca-phe-sua-da',N'Size Lớn (L)',6000),
(N'bac-xiu',N'Size Vừa (M)',0),
(N'bac-xiu',N'Size Lớn (L)',6000),
(N'latte-da',N'Size Vừa (M)',0),
(N'latte-da',N'Size Lớn (L)',6000),
(N'caramel-macchiato',N'Size Vừa (M)',0),
(N'caramel-macchiato',N'Size Lớn (L)',6000),
(N'cold-brew-sua-tuoi',N'Size Vừa (M)',0),
(N'cold-brew-sua-tuoi',N'Size Lớn (L)',6000),
(N'tra-dao-cam-sa',N'Size Vừa (M)',0),
(N'tra-dao-cam-sa',N'Size Lớn (L)',6000),
(N'tra-vai-da-tuyet-yuzu',N'Size Vừa (M)',0),
(N'tra-vai-da-tuyet-yuzu',N'Size Lớn (L)',6000),
(N'tra-den-macchiato',N'Size Vừa (M)',0),
(N'tra-den-macchiato',N'Size Lớn (L)',6000),
(N'tra-sua-oolong-nuong-tran-chau',N'Size Vừa (M)',0),
(N'tra-sua-oolong-nuong-tran-chau',N'Size Lớn (L)',6000),
(N'chocolate-da',N'Size Vừa (M)',0),
(N'chocolate-da',N'Size Lớn (L)',6000),
(N'dao-viet-quat-da-xay',N'Size Vừa (M)',0),
(N'dao-viet-quat-da-xay',N'Size Lớn (L)',6000),
(N'banh-mi-vietnam-thit-nguoi',N'Bình thường',0),
(N'banh-mi-vietnam-thit-nguoi',N'Nướng nóng giòn',0),
(N'banh-mi-gay-ca-ngu-mayo',N'Bình thường',0),
(N'banh-mi-gay-ca-ngu-mayo',N'Nướng nóng giòn',0),
(N'croissant-trung-muoi',N'Bình thường',0),
(N'croissant-trung-muoi',N'Nướng nóng giòn',0),
(N'hong-tra-sua-tran-chau',N'Size Vừa (M)',0),
(N'hong-tra-sua-tran-chau',N'Size Lớn (L)',6000),
(N'tra-sua-mac-ca',N'Size Vừa (M)',0),
(N'tra-sua-mac-ca',N'Size Lớn (L)',6000),
(N'oolong-nuong-kem-dua-da-xay',N'Size Vừa (M)',0),
(N'oolong-nuong-kem-dua-da-xay',N'Size Lớn (L)',6000),
(N'tra-phuc-bon-tu',N'Size Vừa (M)',0),
(N'tra-phuc-bon-tu',N'Size Lớn (L)',6000),
(N'banh-mi-gay-ga-kim-quat',N'Bình thường',0),
(N'banh-mi-gay-ga-kim-quat',N'Nướng nóng giòn',0);

UPDATE t SET Price=s.Price, UpdatedAt=@Now FROM dbo.PropertyProducts t
JOIN dbo.Products p ON p.Id=t.Product_Id JOIN @Properties s ON s.ProductSlug=p.Slug AND s.Name=t.Name;
INSERT dbo.PropertyProducts (Name,Price,Status,Product_Id,CreatedAt,UpdatedAt)
SELECT s.Name,s.Price,N'1',p.Id,@Now,@Now FROM @Properties s JOIN dbo.Products p ON p.Slug=s.ProductSlug
WHERE NOT EXISTS (SELECT 1 FROM dbo.PropertyProducts t WHERE t.Product_Id=p.Id AND t.Name=s.Name);

DECLARE @Options TABLE (ProductSlug nvarchar(200), Name nvarchar(200), Price decimal(18,2), PRIMARY KEY(ProductSlug,Name));
INSERT INTO @Options VALUES
(N'ca-phe-den-da',N'Thêm Shot Espresso',10000),
(N'ca-phe-den-da',N'Thêm Kem Cheese',12000),
(N'ca-phe-sua-da',N'Thêm Shot Espresso',10000),
(N'ca-phe-sua-da',N'Thêm Kem Cheese',12000),
(N'bac-xiu',N'Thêm Shot Espresso',10000),
(N'bac-xiu',N'Thêm Kem Cheese',12000),
(N'latte-da',N'Thêm Shot Espresso',10000),
(N'latte-da',N'Thêm Kem Cheese',12000),
(N'caramel-macchiato',N'Thêm Shot Espresso',10000),
(N'caramel-macchiato',N'Thêm Kem Cheese',12000),
(N'cold-brew-sua-tuoi',N'Thêm Shot Espresso',10000),
(N'cold-brew-sua-tuoi',N'Thêm Kem Cheese',12000),
(N'tra-dao-cam-sa',N'Thêm Kem Cheese Macchiato',12000),
(N'tra-dao-cam-sa',N'Thêm Thạch Đào Giòn',10000),
(N'tra-dao-cam-sa',N'Thêm Trân Châu Trắng',10000),
(N'tra-vai-da-tuyet-yuzu',N'Thêm Kem Cheese Macchiato',12000),
(N'tra-vai-da-tuyet-yuzu',N'Thêm Thạch Đào Giòn',10000),
(N'tra-vai-da-tuyet-yuzu',N'Thêm Trân Châu Trắng',10000),
(N'tra-den-macchiato',N'Thêm Kem Cheese Macchiato',12000),
(N'tra-den-macchiato',N'Thêm Thạch Đào Giòn',10000),
(N'tra-den-macchiato',N'Thêm Trân Châu Trắng',10000),
(N'tra-sua-oolong-nuong-tran-chau',N'Thêm Trân Châu Trắng',10000),
(N'tra-sua-oolong-nuong-tran-chau',N'Thêm Pudding Trứng',10000),
(N'tra-sua-oolong-nuong-tran-chau',N'Thêm Kem Cheese',12000),
(N'chocolate-da',N'Thêm Trân Châu Trắng',10000),
(N'chocolate-da',N'Thêm Pudding Trứng',10000),
(N'chocolate-da',N'Thêm Kem Cheese',12000),
(N'dao-viet-quat-da-xay',N'Thêm Trân Châu Trắng',10000),
(N'dao-viet-quat-da-xay',N'Thêm Pudding Trứng',10000),
(N'dao-viet-quat-da-xay',N'Thêm Kem Cheese',12000),
(N'banh-mi-vietnam-thit-nguoi',N'Thêm Sốt Phô Mai',8000),
(N'banh-mi-vietnam-thit-nguoi',N'Thêm Chà Bông Cay',6000),
(N'banh-mi-gay-ca-ngu-mayo',N'Thêm Sốt Phô Mai',8000),
(N'banh-mi-gay-ca-ngu-mayo',N'Thêm Chà Bông Cay',6000),
(N'croissant-trung-muoi',N'Thêm Sốt Phô Mai',8000),
(N'croissant-trung-muoi',N'Thêm Chà Bông Cay',6000),
(N'hong-tra-sua-tran-chau',N'Thêm Trân Châu Trắng',10000),
(N'hong-tra-sua-tran-chau',N'Thêm Pudding Trứng',10000),
(N'hong-tra-sua-tran-chau',N'Thêm Kem Cheese',12000),
(N'tra-sua-mac-ca',N'Thêm Trân Châu Trắng',10000),
(N'tra-sua-mac-ca',N'Thêm Pudding Trứng',10000),
(N'tra-sua-mac-ca',N'Thêm Kem Cheese',12000),
(N'oolong-nuong-kem-dua-da-xay',N'Thêm Trân Châu Trắng',10000),
(N'oolong-nuong-kem-dua-da-xay',N'Thêm Pudding Trứng',10000),
(N'oolong-nuong-kem-dua-da-xay',N'Thêm Kem Cheese',12000),
(N'tra-phuc-bon-tu',N'Thêm Kem Cheese Macchiato',12000),
(N'tra-phuc-bon-tu',N'Thêm Thạch Đào Giòn',10000),
(N'tra-phuc-bon-tu',N'Thêm Trân Châu Trắng',10000),
(N'banh-mi-gay-ga-kim-quat',N'Thêm Sốt Phô Mai',8000),
(N'banh-mi-gay-ga-kim-quat',N'Thêm Chà Bông Cay',6000);

UPDATE t SET Price=s.Price, UpdatedAt=@Now FROM dbo.OptionProducts t
JOIN dbo.Products p ON p.Id=t.Product_Id JOIN @Options s ON s.ProductSlug=p.Slug AND s.Name=t.Name;
INSERT dbo.OptionProducts (Name,Price,Status,Product_Id,CreatedAt,UpdatedAt)
SELECT s.Name,s.Price,N'1',p.Id,@Now,@Now FROM @Options s JOIN dbo.Products p ON p.Slug=s.ProductSlug
WHERE NOT EXISTS (SELECT 1 FROM dbo.OptionProducts t WHERE t.Product_Id=p.Id AND t.Name=s.Name);

DECLARE @Blogs TABLE (Title nvarchar(300) PRIMARY KEY, Content nvarchar(max), Image nvarchar(max), DaysAgo int);
INSERT INTO @Blogs VALUES
(N'Câu chuyện hạt cà phê LalaCoffee - Hương vị từ đất đỏ Tây Nguyên',N'Một ly cà phê có thể bắt đầu ngày mới, mở đầu một cuộc trò chuyện hoặc đơn giản là dành cho bạn vài phút nghỉ giữa lịch trình bận rộn. Với LalaCoffee, điều đáng nhớ nằm ở cách hương thơm, vị đắng và hậu vị cùng tạo nên một trải nghiệm quen mà vẫn có điều để khám phá.
<h2>Từ hạt cà phê đến hương vị trong ly</h2><p>Nhắc đến cà phê Việt Nam, nhiều người nghĩ ngay đến Tây Nguyên và những ly cà phê đậm vị. Tuy nhiên, cảm nhận cuối cùng còn phụ thuộc vào giống hạt, mức rang, độ xay và cách pha. Một nền cà phê có hương rang rõ sẽ cho cảm giác khác khi uống đen, pha với sữa đặc hay kết hợp cùng sữa tươi. Thay vì tìm một công thức phù hợp với tất cả mọi người, bạn có thể bắt đầu từ hương vị mình yêu thích.</p><h2>Khám phá bằng những lựa chọn quen thuộc</h2><p>Nếu muốn cảm nhận cà phê trực tiếp, hãy thử đen đá và nhấp chậm để nhận ra sự thay đổi từ đầu ngụm đến hậu vị. Cà phê sữa đá phù hợp khi bạn thích độ ngọt béo cân bằng. Bạc xỉu đặt sữa ở vị trí nổi bật, còn latte mang sắc thái mềm mại của espresso kết hợp sữa tươi. Mỗi món là một góc nhìn khác nhau về cùng niềm yêu thích cà phê.</p><h2>Dành thời gian cho ly cà phê của mình</h2><p>Một gợi ý nhỏ là thử món nguyên bản trước khi thêm topping, sau đó điều chỉnh ở lần tiếp theo. Bạn cũng có thể dùng cùng bánh mì hoặc croissant để nhận ra cách món ăn làm thay đổi cảm nhận vị giác. LalaCoffee mong mỗi lần lựa chọn đều giúp bạn hiểu gu của mình thêm một chút, để ly cà phê trở thành khoảng nghỉ dễ chịu trong ngày.</p>',N'/Content/Layout/User/images/blog1.jpg',21),
(N'Nghệ thuật pha chế Cold Brew: Tinh tế và đậm đà trong từng giọt',N'Cold Brew là một lựa chọn thú vị khi bạn đã quen với cà phê pha nóng rồi thêm đá. Cách chiết xuất chậm bằng nước mát hoặc lạnh tạo nên một hướng thưởng thức riêng, nơi thời gian, tỷ lệ cà phê và cách phục vụ đều góp phần định hình hương vị.
<h2>Ủ lạnh khác gì cà phê đá?</h2><p>Cà phê đá thường được pha bằng nước nóng trước khi làm lạnh, trong khi Cold Brew tiếp xúc với nước ở nhiệt độ thấp hơn trong thời gian dài. Tùy công thức, thời gian ủ có thể kéo dài nhiều giờ. Sự khác biệt này ảnh hưởng đến cách hương thơm và các chất trong cà phê được chiết xuất. Vì vậy, không nên xem Cold Brew chỉ là một ly cà phê thông thường được làm lạnh.</p><h2>Khi cà phê gặp sữa tươi</h2><p>Trong menu LalaCoffee, Cold Brew Sữa Tươi là cách làm quen dễ dàng với nhóm đồ uống này. Sữa giúp vị cà phê mềm hơn và bổ sung cảm giác béo nhẹ. Bạn nên khuấy đều trước khi uống để các thành phần hòa quyện, rồi nhấp từng ngụm nhỏ. Khi đá tan, tỷ lệ nước thay đổi và cảm nhận cũng sẽ khác, nên thưởng thức sớm giúp bạn trải nghiệm rõ hơn kết cấu ban đầu.</p><h2>Chọn theo khẩu vị, đừng chỉ theo tên gọi</h2><p>Nếu bạn thích vị cà phê đậm và ngọt rõ, sữa đá vẫn là một lựa chọn gần gũi. Nếu muốn khám phá một sắc thái khác với sữa tươi, hãy thử Cold Brew. Hương vị dịu không đồng nghĩa với việc món không chứa caffeine; bạn nên chọn lượng dùng theo thói quen cá nhân. Kết hợp cùng một phần bánh nhỏ là cách đơn giản để có một giờ nghỉ trọn vẹn.</p>',N'/Content/Layout/User/images/blog2.jpg',18),
(N'Không gian hẹn hò & làm việc đầy cảm hứng tại LalaCoffee',N'Một buổi hẹn cà phê dễ chịu đôi khi bắt đầu từ những điều rất nhỏ: chọn đúng món mình thích, có thời gian trò chuyện và tạm đặt điện thoại sang một bên. LalaCoffee gợi ý vài cách để bạn biến khoảng nghỉ quen thuộc thành một trải nghiệm có chủ đích hơn.
<h2>Một buổi gặp gỡ không cần quá nhiều kế hoạch</h2><p>Khi đi cùng bạn bè, mỗi người có thể chọn một nhóm đồ uống khác nhau rồi chia sẻ cảm nhận: người thích cà phê đậm, người thích trà trái cây, người lại muốn một món đá xay. Một phần bánh mặn hoặc vài vị mochi giúp câu chuyện có thêm điểm bắt đầu. Bạn không cần chọn món giống nhau để có chung một buổi hẹn đáng nhớ.</p><h2>Dành một khoảng thời gian cho bản thân</h2><p>Nếu đi một mình, hãy thử mang theo cuốn sách đang đọc hoặc một danh sách việc ngắn. Chọn đồ uống phù hợp với nhịp nghỉ của bạn: latte để nhâm nhi, trà đào cam sả để đổi vị hoặc chocolate cho một buổi chiều muốn chút ngọt. Trước khi cần ngồi làm việc lâu hay đi cùng nhóm đông, bạn nên liên hệ quán để hỏi tình trạng chỗ ngồi và tiện ích phù hợp.</p><h2>Để hương vị dẫn dắt trải nghiệm</h2><p>Một cách làm mới buổi cà phê là chọn món theo tâm trạng thay vì luôn gọi món quen. Bắt đầu bằng mô tả trong menu, cân nhắc size và phần ăn kèm, sau đó dành vài phút cảm nhận trước khi quay lại công việc. Dù ghé nhanh hay ngồi trò chuyện, điều quan trọng vẫn là một khoảng thời gian vừa vặn với nhu cầu của bạn.</p>',N'/Content/Layout/User/images/blog3.jpg',15),
(N'Chọn cà phê theo gu: Đen đá, sữa đá, bạc xỉu hay latte?',N'Đứng trước menu cà phê, bạn có thể phân vân giữa những tên món rất quen nhưng trải nghiệm lại khác nhau. Cách dễ nhất để chọn là nghĩ đến ba yếu tố: bạn muốn vị cà phê rõ đến đâu, thích loại sữa nào và mong đợi độ ngọt như thế nào.
<h2>Đen đá và sữa đá: Hai nét quen của cà phê Việt</h2><p>Đen đá phù hợp khi bạn thích hương cà phê hiện diện trực tiếp và muốn cảm nhận vị đắng cùng hậu vị rõ ràng. Sữa đá bổ sung độ ngọt béo của sữa đặc, khiến tổng thể tròn hơn mà vẫn giữ nét đậm của cà phê. Nếu đang làm quen với menu, hai món này là điểm bắt đầu dễ so sánh vì khác biệt về sữa rất rõ.</p><h2>Bạc xỉu và latte: Khi bạn thích vị sữa</h2><p>Bạc xỉu thiên về sữa, điểm thêm hương cà phê để tạo cảm giác thơm nhẹ. Latte kết hợp espresso với sữa tươi, thường đem lại trải nghiệm mềm và cân bằng. Hai món đều có cà phê, vì vậy vị dịu không có nghĩa là hoàn toàn không chứa caffeine. Nếu muốn tăng hương cà phê, hãy xem tùy chọn thêm shot thay vì chỉ đổi sang size lớn.</p><h2>Thử một thay đổi mỗi lần</h2><p>Bạn có thể giữ nguyên món và chỉ đổi size hoặc thêm một tùy chọn để nhận ra điều gì phù hợp với mình. Tránh thêm quá nhiều thành phần ngay lần đầu nếu mục tiêu là hiểu hương vị nền. Ghi nhớ món khiến bạn thấy dễ chịu nhất sau khi uống, rồi dùng đó làm điểm xuất phát cho lần khám phá tiếp theo. Một ly cà phê hợp gu luôn đáng giá hơn việc chạy theo tên món cầu kỳ.</p>',N'/Content/Layout/User/images/blog1.jpg',12),
(N'Một vòng menu trà: Tìm hương vị cho buổi chiều của bạn',N'Trà trái cây, trà sữa và trà macchiato cùng có nền trà nhưng mang đến những trải nghiệm rất khác. Nếu bạn đang muốn đổi món cho buổi chiều, hãy bắt đầu bằng việc chọn giữa cảm giác thanh mát của trái cây và độ mềm béo của sữa hoặc kem.
<h2>Nhóm trà trái cây: Chọn theo hương bạn thích</h2><p>Trà Đào Cam Sả kết hợp vị đào quen thuộc với cam chua nhẹ và hương sả. Trà Vải Đá Tuyết Yuzu thiên về hương vải cùng nét cam quýt, còn Trà Phúc Bồn Tử có sắc thái quả mọng chua ngọt. Mỗi món có điểm nhấn riêng; bạn có thể đọc mô tả để chọn hương gần với sở thích thay vì chỉ dựa vào màu sắc của đồ uống.</p><h2>Nhóm trà sữa và kem: Chú ý nền trà</h2><p>Hồng Trà Sữa Trân Châu là lựa chọn cơ bản để làm quen với sự kết hợp giữa trà và sữa. Oolong nướng mang nét hương rang, còn mắc ca bổ sung cảm giác béo bùi. Nếu thích thưởng thức từng lớp, Trà Đen Macchiato cho phép bạn cảm nhận kem trước rồi đến trà. Khuấy đều sẽ tạo một trải nghiệm khác, mềm và đồng nhất hơn.</p><h2>Topping là điểm nhấn cuối cùng</h2><p>Trân châu trắng tạo độ dai, pudding thiên về mềm béo, còn kem cheese bổ sung hương sữa và chút mặn nhẹ. Hãy thử một loại mỗi lần để vẫn nhận ra vị trà ban đầu. Với món đã có trân châu trong tên, phần thêm là lượng bổ sung theo nhu cầu của bạn. Chọn món vừa đủ sẽ giúp giờ nghỉ thoải mái hơn và giữ được nét riêng của từng ly trà.</p>',N'/Content/Layout/User/images/blog2.jpg',9),
(N'Bánh và cà phê: Những cặp hương vị đáng thử',N'Một phần bánh phù hợp có thể khiến ly cà phê quen trở nên mới mẻ hơn. Bạn không cần ghi nhớ những quy tắc phức tạp: chỉ cần cân nhắc món đang thiên về ngọt, béo hay mặn rồi chọn đồ uống giúp cân bằng cảm giác ấy.
<h2>Bánh mì cho một bữa ăn nhẹ</h2><p>Bánh mì thịt nguội có nhiều lớp nhân và đồ chua, phù hợp khi bạn muốn phần ăn đầy đặn hơn. Bánh mì gậy cá ngừ mayo thiên về béo mềm, còn gà kim quất tạo điểm nhấn chua nhẹ ở phần sốt. Cà phê sữa đá là lựa chọn quen cho buổi sáng; trà trái cây lại thích hợp nếu bạn muốn đổi hướng hương vị. Bạn có thể chọn nướng nóng khi đặt để phần vỏ bánh thơm và giòn hơn.</p><h2>Croissant và ly cà phê cân bằng</h2><p>Croissant trứng muối mang hương bơ và nhân đậm vị, vì thế latte hoặc cà phê đen là những gợi ý dễ thử. Latte tạo cảm giác mềm mại xuyên suốt, còn cà phê đen giúp độ béo của bánh bớt nổi bật. Hãy thử xen kẽ một miếng bánh với một ngụm đồ uống để cảm nhận cách hậu vị thay đổi.</p><h2>Mochi cho điểm kết mát dịu</h2><p>Matcha, socola, việt quất và phúc bồn tử mang đến bốn sắc thái khác nhau trong cùng kết cấu vỏ dẻo, nhân kem. Bạn có thể chọn một vị quen và một vị mới để chia sẻ. Nên thưởng thức sớm sau khi nhận để giữ phần nhân mát lạnh. Nếu có nhu cầu tránh nguyên liệu cụ thể như sữa, trứng, lúa mì hoặc hạt, hãy hỏi quán trước khi chọn món thay vì chỉ suy đoán từ tên gọi.</p>',N'/Content/Layout/User/images/blog3.jpg',6),
(N'Tự chọn size và topping để ly nước hợp gu hơn',N'Size và topping giúp bạn điều chỉnh trải nghiệm, nhưng càng nhiều lựa chọn không có nghĩa là càng dễ tìm được món hợp gu. Một cách đơn giản là chọn hương vị nền trước, sau đó thêm đúng thành phần bạn đang muốn nhấn mạnh.
<h2>Chọn size theo thời gian thưởng thức</h2><p>Size vừa phù hợp khi bạn muốn dùng một ly gọn trong giờ nghỉ ngắn hoặc thử món lần đầu. Size lớn dành cho lúc muốn nhâm nhi lâu hơn, nhưng đá tan cũng sẽ làm thay đổi cảm nhận theo thời gian. Khi chọn trên trang chi tiết, hãy xem phần phụ thu của từng kích cỡ để biết giá cuối cùng trước khi thêm vào giỏ hàng.</p><h2>Chọn topping theo kết cấu</h2><p>Nếu thích cảm giác dai nhẹ, trân châu trắng là một lựa chọn dễ làm quen. Pudding trứng đem lại độ mềm béo, trong khi kem cheese bổ sung lớp hương sữa và vị mặn nhẹ. Với cà phê, thêm shot espresso giúp hương cà phê rõ hơn; đây là thay đổi khác với việc tăng kích cỡ. Những tùy chọn hiển thị phụ thuộc từng món, vì vậy hãy xem trực tiếp ở trang chi tiết thay vì mặc định mọi món đều giống nhau.</p><h2>Kiểm tra lại trước khi đặt</h2><p>Một món có thể đã có thành phần như trân châu ngay trong công thức; phần topping thêm là lựa chọn bổ sung. Sau khi thêm vào giỏ, bạn nên kiểm tra tên món, kích cỡ, topping, số lượng và thành tiền. Nếu có yêu cầu riêng chưa có trên menu, hãy liên hệ quán để hỏi khả năng đáp ứng. Điều chỉnh từng bước giúp bạn hiểu điều mình thích và tránh một ly nước có quá nhiều hương vị chồng lên nhau.</p>',N'/Content/Layout/User/images/blog1.jpg',3),
(N'Đặt món tại LalaCoffee: Từ chọn món đến nhận đơn',N'Đặt món trực tuyến sẽ thuận tiện hơn khi bạn dành một chút thời gian kiểm tra lựa chọn trước lúc xác nhận. Từ trang menu đến thông tin giao nhận, mỗi bước đều giúp quán hiểu đúng món bạn muốn và cách liên hệ khi cần.
<h2>Bắt đầu từ menu và trang chi tiết</h2><p>Bạn có thể duyệt theo danh mục hoặc mở một món để xem giá, mô tả và ảnh. Trang chi tiết cho biết các kích cỡ và tùy chọn đang có của món đó. Chọn số lượng phù hợp rồi thêm vào giỏ; nếu muốn đổi lựa chọn, hãy xem lại thông tin món trước khi tiếp tục. Với bánh hoặc đồ uống có nguyên liệu bạn cần tránh, nên hỏi quán trước khi đặt.</p><h2>Rà soát giỏ và thông tin nhận hàng</h2><p>Trong giỏ hàng, kiểm tra tên món, số lượng và thành tiền. Ở bước thanh toán, nhập họ tên, số điện thoại và địa chỉ đủ rõ để quán có thể liên hệ và xử lý đơn. Nếu có ghi chú, hãy viết ngắn gọn, cụ thể. Khả năng đáp ứng một yêu cầu ngoài các lựa chọn sẵn có cần được quán xác nhận, vì vậy không nên xem ghi chú là cam kết tự động.</p><h2>Lưu lại mã đơn và kiểm tra trạng thái</h2><p>Sau khi đặt thành công, hãy lưu mã đơn để tra cứu khi cần. Nếu chọn phương thức thanh toán trực tuyến, làm theo hướng dẫn hiển thị và kiểm tra kết quả cuối cùng; việc đóng cửa sổ thanh toán giữa chừng có thể khiến giao dịch chưa hoàn tất. Khi cần hỗ trợ, cung cấp mã đơn cùng thông tin liên hệ đã dùng sẽ giúp quá trình kiểm tra thuận tiện hơn. Cuối cùng, đừng quên xem lại đơn trước khi gửi để buổi thưởng thức bắt đầu thật suôn sẻ.</p>',N'/Content/Layout/User/images/blog2.jpg',0);

IF EXISTS (SELECT b.Title FROM dbo.Blogs b JOIN @Blogs s ON s.Title=b.Title GROUP BY b.Title HAVING COUNT(*)>1)
    THROW 51003, N'Bài viết trùng tiêu đề trong bộ seed. Hãy xử lý dữ liệu trùng trước.', 1;
UPDATE b SET Content=s.Content, Image=s.Image FROM dbo.Blogs b JOIN @Blogs s ON s.Title=b.Title;
INSERT dbo.Blogs (Title,Content,Image,CreatedDate,Published,Author)
SELECT s.Title,s.Content,s.Image,DATEADD(day,-s.DaysAgo,@Now),1,N'LalaCoffee Team' FROM @Blogs s
WHERE NOT EXISTS (SELECT 1 FROM dbo.Blogs b WHERE b.Title=s.Title);

-- Chỉ tạo tài khoản mẫu còn thiếu, không đặt lại mật khẩu hoặc nâng quyền tài khoản đã có.
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name=N'Admin')
    INSERT dbo.AspNetRoles (Id,Name,Discriminator) VALUES (CONVERT(nvarchar(128),NEWID()),N'Admin',N'IdentityRole');
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name=N'User')
    INSERT dbo.AspNetRoles (Id,Name,Discriminator) VALUES (CONVERT(nvarchar(128),NEWID()),N'User',N'IdentityRole');
DECLARE @Accounts TABLE (UserName nvarchar(256), Email nvarchar(256), PasswordHash nvarchar(max), RoleName nvarchar(256));
INSERT INTO @Accounts VALUES
(N'admin',N'admin@lalacoffee.vn',N'AKdgfQL6E5xmRrV/3I8IUVGpEgMyhTUtbypGQsTiIcFfoI8vhVoNc46w4BZ7kU+M0A==',N'Admin'),
(N'user',N'user@lalacoffee.vn',N'AO/jeKDdiNQSL6rU8PMhalHFDHrIdy/LGeSRp32h9Hy/PMPwMmUb8/K8PohCFb62MA==',N'User');
DECLARE @NewUsers TABLE (Id nvarchar(128), UserName nvarchar(256));
INSERT dbo.AspNetUsers (Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled,AccessFailedCount,UserName)
OUTPUT inserted.Id,inserted.UserName INTO @NewUsers
SELECT CONVERT(nvarchar(128),NEWID()),a.Email,1,a.PasswordHash,CONVERT(nvarchar(128),NEWID()),0,0,1,0,a.UserName
FROM @Accounts a WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers u WHERE u.UserName=a.UserName OR u.Email=a.Email);
INSERT dbo.AspNetUserRoles (UserId,RoleId)
SELECT u.Id,r.Id FROM @NewUsers u JOIN @Accounts a ON a.UserName=u.UserName JOIN dbo.AspNetRoles r ON r.Name=a.RoleName;

    COMMIT TRANSACTION;
    PRINT N'LalaCoffee: seed hoàn tất.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
