# ReThreads – Tài liệu Business Rules

> Phiên bản rà soát: 05/08/2026  
> Phạm vi: mã nguồn backend ASP.NET Core trong `CapstoneProject` và các workflow đã tích hợp với frontend.  
> Mục đích: mô tả những luật nghiệp vụ **đã được implement thực sự**, mức độ hoàn thiện theo role, và các luật nên bổ sung để vận hành như một doanh nghiệp.

## 1. Cách đọc tài liệu

Mỗi luật được gắn một mức độ:

- **Đã enforce ở BE**: backend kiểm tra và từ chối thao tác không hợp lệ; đây là business rule đáng tin cậy nhất.
- **Đã có flow/API**: có endpoint và thay đổi dữ liệu đầy đủ, nhưng một số kiểm tra phụ thuộc UI hoặc cấu hình.
- **Mới một phần**: có model/generic CRUD hoặc màn hình, chưa đủ một workflow khép kín.
- **Đề xuất**: chưa được implement, nên bổ sung cho môi trường thực tế.

Lưu ý: việc một rule xuất hiện trên UI không đồng nghĩa rule đã an toàn. Rule quan trọng phải được kiểm tra lại ở backend.

## 2. Role và phạm vi quyền hiện tại

| Role | Chức năng đã có | Mức độ |
|---|---|---|
| Donor | Đăng ký, xác thực email, tạo/sửa/hủy/xem Donation Request, nhận notification theo hành trình | Đã có flow/API |
| Manager | Dashboard, tài khoản, ca làm, team, điều phối đơn, cấu hình phân loại, cấu trúc kho, duyệt Distribution Request | Đã có flow/API |
| ReceivingStaff | Xem team/ca/batch, bắt đầu ca, nhận hoặc từ chối/dời lịch, nhận tại kho, hoàn tất và gửi phân loại | Đã có flow/API |
| ClassificationStaff | Xác nhận nhận Intake Batch, phân loại item, gom Classified Batch, gửi kho | Đã có flow/API |
| WarehouseStaff | Nhận batch, đối chiếu, put-away, tồn kho, di chuyển, xuất kho, Distribution và GHN | Đã có flow/API |
| CharityOrganization | Xem tồn từ thiện, tạo/sửa/xóa yêu cầu trước duyệt, theo dõi GHN | Đã có flow/API |
| RecyclingOrganization | Có role và quyền generic trên một số entity phân phối | Mới một phần; chưa có workflow tái chế chuyên biệt |
| Admin | Có thể tồn tại trong dữ liệu/thiết kế ban đầu | Chưa có workflow quản trị riêng được enforce trong controller hiện tại |

## 3. Luật dùng chung toàn hệ thống

### 3.1 Xác thực và phân quyền

1. API nghiệp vụ yêu cầu JWT; token chứa `UserId`, họ tên, role và username. **Đã enforce ở BE**.
2. Token đăng nhập có thời hạn 2 giờ. **Đã enforce ở BE**.
3. Tài khoản chỉ đăng nhập được khi:
   - mật khẩu BCrypt đúng;
   - `EmailConfirmed = true`;
   - `UserStatus = Active`.
4. Controller sử dụng role-based authorization; ví dụ Manager không thể gọi endpoint Receiving Staff chỉ bằng việc ẩn nút trên UI.
5. Dữ liệu dùng soft delete phổ biến qua `IsActive`, `DeleteAt`, `DeletedBy`; truy vấn thông thường bỏ qua bản ghi `IsActive = false`.
6. Notification thuộc riêng từng user; user có thể đọc một thông báo, đọc tất cả hoặc xóa tất cả thông báo của mình.

### 3.2 Mã nghiệp vụ và truy xuất nguồn gốc

1. Donation Request có mã duy nhất dạng `DR-{YEAR}-{8 ký tự GUID}` và được lưu trong `DonationRequests.RequestCode`.
2. Intake Batch có mã dạng `INT-{yyyyMMdd}-{chuỗi ngẫu nhiên}` và được lưu trong DB.
3. Classified Batch có mã dạng `CB-{yyyyMMdd}-{A|B|C}-{chuỗi ngẫu nhiên}` và được lưu trong DB.
4. Distribution Request có mã nghiệp vụ `DIST-...`; phiếu xuất có mã `PXK-...`; transaction có mã `TX-...`.
5. Quan hệ truy xuất nguồn gốc:
   - Donation Request ↔ Intake Batch: `IntakeBatchDonationRequests`;
   - Classified Batch ↔ Donation Request: `ClassifiedBatchDonationRequests`;
   - Classified Batch → Inventory → Transaction/Distribution.
6. Một Classified Batch có thể chứa nguồn từ nhiều Donation Request; hệ thống không gắn cứng một DR duy nhất cho từng item khi không thể xác minh vật lý item thuộc đơn nào.

### 3.3 Notification

1. Donor tạo đơn → tất cả Manager active nhận notification và link tới màn hình điều phối.
2. Donor nhận notification khi đơn được nhận, gửi phân loại, bộ phận phân loại nhận, hoàn tất phân loại, gửi kho, nhập kho và xuất cho tổ chức (tùy bước gọi `NotificationWriter`).
3. Organization nhận notification khi Manager duyệt/từ chối và khi tiến trình phân phối thay đổi.
4. Warehouse Staff nhận notification khi yêu cầu phân phối được Manager duyệt.
5. Notification chứa `TargetUrl`; click có thể chuyển tới trang chi tiết liên quan.

## 4. Business rules theo role

## 4.1 Donor

### A. Đăng ký và xác thực email

1. Username dài 3–30 ký tự, chỉ gồm chữ, số, dấu chấm hoặc gạch dưới. **Đã enforce ở BE**.
2. Username không được trùng, không phân biệt hoa thường.
3. Email phải đúng định dạng, tối đa 254 ký tự và không được trùng.
4. Số điện thoại phải là số di động Việt Nam hợp lệ (`03/05/07/08/09` hoặc `+84`) và không được trùng.
5. Họ tên dài 2–100 ký tự.
6. Mật khẩu tối thiểu 8 ký tự, có chữ hoa, chữ số và ký tự đặc biệt.
7. Tài khoản đăng ký mới có trạng thái `PendingVerification`, chưa active.
8. OTP email gồm 6 chữ số, lưu dưới dạng hash, có hiệu lực 5 phút.
9. Nhập sai tối đa 5 lần; sau đó phải xin mã mới.
10. Chỉ được gửi lại mã sau tối thiểu 1 phút; mã cũ active sẽ bị vô hiệu hóa.
11. Xác thực thành công → `EmailConfirmed = true`, `UserStatus = Active`, `IsActive = true`.
12. SMS verification đã được loại khỏi flow; hệ thống hiện xác thực bằng email.

### B. Tạo Donation Request

1. Donor phải chọn một kho active/tồn tại.
2. `ContactName` được lấy từ form và bắt buộc; không ép lấy từ JWT.
3. `ContactPhoneNumber` được lấy từ form, chuẩn hóa chỉ còn chữ số, phải có 10 số và bắt đầu bằng `0`.
4. Chỉ chấp nhận hai phương thức:
   - `StaffPickup`: nhân viên đến địa chỉ donor;
   - `DonorDropOff`: donor tự mang đến kho.
5. `StaffPickup` bắt buộc có địa chỉ lấy hàng và ngày hẹn.
6. `DonorDropOff` lưu địa chỉ kho làm điểm tiếp nhận.
7. Trước 11:00 giờ Việt Nam có thể chọn ngày hiện tại; từ 11:00 trở đi ngày sớm nhất là ngày kế tiếp. **Đã enforce ở BE**.
8. `StaffPickup` khởi tạo trạng thái `WaitingReceivingStaff`; `DonorDropOff` khởi tạo `PendingStaffAssign`.
9. Tạo đơn không tự động phân công team; Manager phải phân công thủ công hoặc điều phối tự động.
10. Khi tạo thành công, Manager nhận notification có mã DR, ngày yêu cầu và link điều phối.

### C. Sửa, hủy và xem đơn

1. Donor chỉ sửa/hủy được đơn của chính mình.
2. Chỉ sửa/hủy khi trạng thái là `PendingStaffAssign` hoặc `WaitingReceivingStaff`.
3. Hủy đơn chuyển trạng thái `Cancelled`, lưu lý do `Cancelled by donor`.
4. Donor xem danh sách đơn của chính mình bằng danh tính JWT.
5. Endpoint tìm theo số điện thoại vẫn tồn tại và yêu cầu role Donor; về mặt sản phẩm UI chính nên ưu tiên “Đơn của tôi” để tránh lộ dữ liệu.

## 4.2 Manager

### A. Quản lý tài khoản

1. Manager được tạo/quản lý các role: Donor, CharityOrganization, RecyclingOrganization, ReceivingStaff, ClassificationStaff, WarehouseStaff.
2. Manager không được quản lý tài khoản Admin hoặc Manager qua service này.
3. Username Manager tạo phải dài 4–50 ký tự và đúng tập ký tự cho phép.
4. Email, số điện thoại, username phải duy nhất.
5. Họ tên, địa chỉ và mật khẩu ban đầu là bắt buộc; mật khẩu tuân thủ policy mạnh.
6. ReceivingStaff, ClassificationStaff và WarehouseStaff bắt buộc được gắn với một kho active.
7. Donor/Organization không bị gắn `WarehouseId` dù client gửi lên.
8. Tài khoản do Manager tạo được active và email-confirmed ngay.
9. Khóa tài khoản là soft lock: `UserStatus = Inactive`; mở khóa chuyển lại `Active`.
10. Xóa tài khoản là soft delete: `IsActive = false`, `UserStatus = Deleted`.

### B. Quản lý ca làm

1. Manager chỉ tạo ca cho kho tồn tại.
2. Lịch năm chỉ chấp nhận năm 2020–2100.
3. Manager chọn các thứ làm việc trong tuần; giá trị phải thuộc enum ngày hợp lệ.
4. Manager cấu hình giờ bắt đầu/kết thúc ca sáng và chiều.
5. Giờ kết thúc phải sau giờ bắt đầu; ca sáng phải kết thúc trước khi ca chiều bắt đầu.
6. Ngày nghỉ mặc định và ngày lễ bổ sung được loại khỏi lịch; ngày lễ bổ sung phải thuộc đúng năm được chọn.
7. Hệ thống vẫn sinh hai bản ghi Shift riêng mỗi ngày để team, batch và trạng thái hai ca độc lập.
8. Không tạo trùng ca cùng kho/ngày/giờ bắt đầu.
9. Chỉ Shift `Scheduled` mới được sửa hoặc xóa.
10. Không cho thời gian ca sửa bị overlap với ca khác trong cùng kho/ngày.
11. Không xóa Shift đã có team, assignment hoặc Intake Batch.
12. “Xóa tất cả lịch năm” chỉ xóa các Shift `Scheduled` chưa bị ràng buộc; ca đã vận hành được bảo vệ.

### C. Quản lý Receiving Team

1. Một team phải có đúng 2 thành viên khác nhau.
2. Thành viên phải là ReceivingStaff active và làm tại cùng kho với Shift.
3. Team có hai loại:
   - `ReceivingPickup`: đi lấy tại địa chỉ;
   - `ReceivingWarehouse`: trực nhận tại kho.
4. Team chỉ được tạo khi Shift còn `Scheduled`.
5. Một nhân viên không được nằm trong hai ca/team có thời gian chồng lấn.
6. Mỗi Shift chỉ có tối đa một team trực kho (`ReceivingWarehouse`); có thể có nhiều pickup team.
7. Chỉ sửa thành viên hoặc xóa team trước khi ca bắt đầu.
8. Không xóa team khi còn request đang gắn; phải chuyển đơn khỏi team trước.

### D. Điều phối Donation Request

1. Chỉ lấy các đơn active, chưa phân công, đúng kho và đúng ngày hẹn.
2. `StaffPickup` chỉ được gắn vào pickup team.
3. `DonorDropOff` chỉ được xử lý bởi warehouse receiving team; không đưa vào tuyến đường pickup.
4. Team và request phải cùng kho.
5. Ngày Shift phải trùng ngày hẹn tiếp nhận.
6. Team nhận đơn phải đủ đúng 2 thành viên.
7. Một request chỉ có một PickupAssignment active.
8. Điều phối tự động cân bằng số đơn giữa các team và ưu tiên nhóm/tuyến địa lý từ địa chỉ; hệ thống tạo Intake Batch cho từng team nếu chưa có.
9. Request tạo trong chính ngày hẹn được ưu tiên cho ca chiều; nếu cần xử lý ca sáng phải có dữ liệu/ngày hẹn phù hợp hoặc điều chỉnh nghiệp vụ có chủ đích.
10. Nếu có đơn phát sinh buổi sáng nhưng thiếu team chiều hoàn chỉnh, auto-balance từ chối và yêu cầu tạo team chiều tương ứng.
11. Chuyển request giữa team chỉ áp dụng assignment `Pending` và vẫn kiểm tra kho, ngày, loại team và trùng ca.
12. Phân công thành công chuyển Donation Request sang `ReceivingStaffAssigned` và gửi notification cho donor.

### E. Dashboard và báo cáo

1. Dashboard cho phép lọc theo kho, năm, tháng và ngày.
2. Năm hợp lệ 2000–2100; tháng 1–12; lọc tháng bắt buộc kèm năm.
3. Chỉ transaction `Posted` được tính vào số liệu nhập/xuất.
4. Dashboard tổng hợp Donation Request, Intake Batch, Classified Batch, tồn kho và transaction theo phạm vi lọc.

### F. Cấu hình phân loại

1. Manager CRUD Category cho: loại vải, nhóm quần áo, loại quần áo, giới tính, đối tượng, size, nhãn A/B/C.
2. Clothing Type bắt buộc thuộc một Garment Group.
3. Sort order bắt đầu từ 1, không vượt quá số vị trí hợp lệ; chọn vị trí đã tồn tại sẽ sắp xếp/swap theo service cấu hình.
4. Ngừng dùng category là soft delete/deactivate để giữ dữ liệu lịch sử.
5. Manager CRUD câu hỏi đánh giá và ba lựa chọn A/B/C.
6. Display order của câu hỏi được quản lý liên tục; thay đổi thứ tự không làm mất kết quả cũ.
7. Ngưỡng tổng hợp nhãn được cấu hình tại Category `ConditionGrade`:
   - xét số câu C trước;
   - nếu chưa đạt C thì xét số câu B;
   - còn lại là A.

### G. Quản lý cấu trúc kho

1. Tên kho dài 3–150 ký tự; địa chỉ dài 10–500 ký tự; capacity > 0 và không vượt giới hạn kỹ thuật.
2. Tên và địa chỉ kho active không được trùng.
3. Khi tạo kho, hệ thống khởi tạo cấu trúc khu từ thiện/tái chế/tiêu hủy theo capacity.
4. Chỉ Manager được thay đổi layout kho.
5. Tên khu không trùng trong cùng kho; tên dãy không trùng trong cùng khu; mã location không trùng trong cùng kho.
6. Tổng capacity các dãy không được vượt capacity khu.
7. Capacity khu/dãy/location không được giảm thấp hơn lượng hàng hiện có hoặc capacity con đã phân bổ.
8. Không được chuyển khu sang kho khác, dãy sang khu khác hoặc location sang dãy khác bằng update thông thường.
9. Chỉ xóa khu/dãy/location khi đã chuyển hoặc xuất hết tồn kho.
10. Trạng thái location chỉ nhận `Available`, `Blocked`, `Maintenance`.

### H. Duyệt Distribution Request

1. Manager chỉ duyệt/từ chối request ở `PendingManagerApproval`.
2. Từ chối → `Rejected`, lưu lý do và báo organization.
3. Duyệt phải kiểm tra lại available quantity của từng inventory.
4. Duyệt làm tăng `ReservedQuantity/ReservedWeight`, chưa trừ tồn thực tế.
5. Sau duyệt → `ApprovedAwaitingWarehouse` và Warehouse Staff được thông báo.

## 4.3 Receiving Staff

### A. Quyền xem dữ liệu

1. Staff chỉ xem batch/team mà mình là thành viên active.
2. Chi tiết batch gồm team, ca, kho, request và thứ tự tuyến.
3. Team trực kho xem danh sách DonorDropOff chung theo kho/ngày của ca phụ trách.

### B. Bắt đầu và kết thúc ca

1. Bắt đầu Intake Batch sẽ chuyển Shift `Scheduled → InProgress` và Batch `Planned → Receiving`.
2. Shift `Completed` không được bắt đầu lại.
3. Chỉ thành viên team thuộc Shift mới được kết thúc ca.
4. Chỉ Shift `InProgress` mới được kết thúc.
5. Không kết thúc ca khi còn PickupAssignment active có trạng thái `Pending`.
6. Khi kết thúc, các batch `Planned/Receiving` của staff được chuyển `Completed`, sau đó Shift chuyển `Completed`.

### C. Xác nhận nhận hàng tại địa chỉ

1. Ca và batch phải đang hoạt động (`InProgress`/`Receiving`).
2. Request phải thuộc route/batch của team và assignment phải `Pending`.
3. Actual weight phải lớn hơn 0.
4. Một Donation Request không được thêm hai lần vào cùng Intake Batch.
5. Xác nhận thành công:
   - assignment → `Received`;
   - lưu giờ/người nhận/ghi chú/ảnh thực nhận;
   - tạo `IntakeBatchDonationRequest`;
   - Donation Request → `Confirmed`;
   - cộng ActualWeight vào TotalWeight của Intake Batch;
   - gửi notification cho donor.

### D. Nhận hàng donor mang tới kho

1. Request phải có `DeliveryMethod = DonorDropOff`, ngày dự kiến và chưa được xử lý.
2. Staff phải thuộc `ReceivingWarehouse` team đang có Shift `InProgress`, cùng kho và đúng ngày.
3. Request không được có assignment active trước đó.
4. Nếu team trực kho chưa có Intake Batch, hệ thống tạo batch đang `Receiving` khi xác nhận donor thực sự đến.
5. Đơn chưa đến không được tính là ca chưa hoàn thành; nó vẫn nằm trong danh sách chờ của kho/ngày cho ca phù hợp.

### E. Dời lịch, từ chối, hoàn tất và bàn giao

1. Chỉ xử lý assignment `Pending` trong ca đang hoạt động.
2. Dời lịch → assignment `Rescheduled` và inactive; Donation Request quay về `WaitingReceivingStaff` với ngày mới.
3. Từ chối → assignment `Cancelled`; Donation Request → `Reject`, lưu lý do.
4. Chỉ hoàn tất Intake Batch khi không còn assignment `Pending`.
5. Chỉ Intake Batch `Completed` và có ít nhất một Donation Request đã nhận mới được gửi phân loại.
6. Gửi phân loại → `SentToClassification`, lưu thời gian/người gửi và báo donor.

## 4.4 Classification Staff

### A. Nhận và bắt đầu phân loại

1. Chỉ thấy Intake Batch có trạng thái liên quan đến phân loại.
2. Chỉ batch `SentToClassification` mới được xác nhận nhận.
3. Xác nhận nhận → `PendingClassification`, lưu staff và thời gian; donor được thông báo.
4. Chỉ batch `PendingClassification` hoặc `Classifying` mới được bắt đầu; batch đã `Classified` không được mở lại.
5. Bắt đầu → `Classifying`.

### B. Phân loại từng item

1. Batch phải ở `Classifying`.
2. Staff phải chọn đầy đủ category: loại vải, nhóm, loại quần áo, giới tính, đối tượng, size.
3. Clothing Type phải thuộc đúng Garment Group.
4. Category được chọn phải tồn tại và active.
5. Mỗi câu hỏi tình trạng phải được trả lời đúng một lần.
6. Answer phải thuộc đúng Question.
7. Ảnh item được frontend yêu cầu trước khi gửi; backend lưu danh sách URL ảnh.
8. Nhãn tổng hợp:
   - đạt ngưỡng số câu C → nhãn C;
   - nếu không, đạt ngưỡng số câu B → nhãn B;
   - còn lại → nhãn A.
9. Hướng xử lý tự động: A → `Charity`, B → `Recycling`, C → `Disposal`.
10. Item được gom vào Classified Batch theo khóa gồm ngày, warehouse, fabric, garment group, clothing type, gender, target user, size, grade và processing direction.
11. Item cùng khóa có thể đến từ nhiều Intake Batch/Donation Request và được cộng số lượng/khối lượng vào cùng batch mở.

### C. Hoàn tất và gửi kho

1. Chỉ batch `Classifying` mới được hoàn tất.
2. Phải có ít nhất một item đã phân loại.
3. Hoàn tất intake classification → `Classified` và báo donor.
4. Grouped Classified Batch chỉ được gửi kho khi `Open` và có item.
5. Gửi kho → `PendingWarehouseReceipt`; gửi hàng loạt bỏ qua batch không còn `Open` nhưng từ chối batch rỗng.
6. Provenance từ Classified Batch về các Donation Request nguồn được giữ khi gửi kho.

## 4.5 Warehouse Staff

### A. Phạm vi kho

1. Warehouse Staff bắt buộc có `WarehouseId`.
2. Staff chỉ truy cập kho được gán; truy cập kho khác bị từ chối.
3. Manager có thể xem/chỉnh layout nhiều kho; Warehouse Staff chỉ vận hành kho của mình.

### B. Xác nhận nhận Classified Batch

1. Batch phải thuộc kho staff và ở `PendingWarehouseReceipt`.
2. Actual item count và actual weight phải > 0.
3. Nếu seal không nguyên vẹn, bắt buộc có discrepancy note.
4. Xác nhận → batch `WarehouseReceived`, tạo Inventory tạm ở trạng thái `AwaitingPutaway` và ghi transaction nhận hàng.

### C. Đề xuất vị trí và put-away

1. Hệ thống chỉ đề xuất vị trí cùng hướng xử lý tương ứng grade.
2. A chỉ vào khu `Charity`, B vào `Recycling`, C vào `Disposal`.
3. Location `Blocked` không được nhận hàng.
4. Location phải đủ capacity còn lại.
5. Chỉ batch đã `WarehouseReceived` mới được put-away.
6. Put-away thành công:
   - Inventory → `Available`;
   - gắn StorageLocation;
   - cập nhật trọng lượng location/khu/kho;
   - Classified Batch → `Stored`;
   - ghi transaction `PUTAWAY` ở trạng thái `Posted`.

### D. Di chuyển và xuất kho

1. Inventory phải tồn tại và còn khả dụng.
2. Di chuyển yêu cầu inventory đã có vị trí; vị trí đích khác nguồn, đủ capacity và cùng khu hướng xử lý.
3. Di chuyển cập nhật trọng lượng cả nguồn/đích và ghi transaction `MOVE`.
4. Xuất kho yêu cầu quantity, weight và reason > 0/hợp lệ.
5. Không được xuất quá `Quantity - ReservedQuantity` hoặc trọng lượng khả dụng.
6. Hết số lượng → Inventory `Depleted`; còn hàng → `Available`.
7. Mọi nhập/xếp vị trí/di chuyển/xuất kho tạo `InventoryTransaction` và `TransactionItem` chứa before/after quantity, weight, vị trí và người thực hiện.

### E. Thực hiện Distribution và GHN

1. Chỉ request `ApprovedAwaitingWarehouse` cùng kho staff mới được lập phiếu xuất.
2. Lập phiếu xuất mới thực sự trừ reserved và tồn kho, cập nhật sức chứa, tạo transaction OUT và mã phiếu `PXK`.
3. Sau xuất → `ReadyForGhn`; donor nguồn được thông báo hàng đã chuyển cho tổ chức kèm lời cảm ơn.
4. Chỉ request `ReadyForGhn` mới được tạo vận đơn.
5. GHN token, ShopId, địa chỉ pickup và mã hành chính phải được cấu hình server-side.
6. Tạo GHN thành công → `GhnBooked`, lưu order code, trạng thái `ready_to_pick` và lịch sử.
7. Đồng bộ GHN chỉ cho Manager, Warehouse Staff cùng kho hoặc Organization sở hữu request.
8. Khi status GHN thay đổi, hệ thống lưu thêm `ShipmentStatusHistory`.

## 4.6 Charity Organization

1. Chỉ xem Inventory `Available`, hướng xử lý `Charity` và số lượng khả dụng > 0.
2. Organization chọn một kho và một hoặc nhiều batch; mỗi inventory chỉ xuất hiện một lần trong request.
3. Recipient name, phone, delivery address và purpose/notes đều bắt buộc; phone phải hợp lệ.
4. Requested quantity phải > 0 và không vượt available quantity.
5. Requested weight được tính theo trọng lượng trung bình/item của inventory.
6. Tạo request → `PendingManagerApproval`, sinh mã DIST và báo Manager.
7. Organization chỉ sửa/xóa request của mình khi còn `PendingManagerApproval`.
8. Xóa request ở bước này không làm thay đổi tồn vì hàng chưa được reserve.
9. Organization xem request của mình và theo dõi GHN; không được xem shipment của organization khác.

## 4.7 Recycling Organization

Hiện role này có thể xuất hiện trong authorization của generic Distribution entities nhưng workflow `DistributionOperations` chỉ mở catalog/tạo request chuyên biệt cho `CharityOrganization`. Do đó các luật sau **chưa được implement đầy đủ**:

- catalog tồn kho `Recycling` riêng;
- tạo yêu cầu tái chế;
- hợp đồng/đơn giá thu mua;
- cân đối khối lượng bàn giao;
- chứng từ tái chế và xác nhận hoàn tất;
- truy xuất chứng nhận môi trường.

## 4.8 Admin

Chưa có controller/service nghiệp vụ Admin riêng. Manager hiện đang đảm nhận phần lớn cấu hình vận hành. Nếu dự án giữ role Admin, cần phân định lại quyền hệ thống và quyền nghiệp vụ ở phần đề xuất.

## 5. State machine đang áp dụng

### 5.1 Donation Request

```text
StaffPickup: WaitingReceivingStaff
DonorDropOff: PendingStaffAssign
        ↓ Manager điều phối hoặc staff trực kho xác nhận
ReceivingStaffAssigned (đối với assignment trước)
        ↓ Receiving Staff nhận thực tế
Confirmed
        ↓ Intake Batch gửi phân loại
SendToClassification/Classifying/Classified/Stored (theo tiến trình liên quan)

Nhánh ngoại lệ:
Pending/Waiting → Cancelled (donor)
Pending assignment → Reject (receiving staff)
Pending assignment → Rescheduled → WaitingReceivingStaff
```

### 5.2 Shift và Intake Batch

```text
Shift: Scheduled → InProgress → Completed
Intake Batch: Planned → Receiving → Completed → SentToClassification
```

### 5.3 Classification

```text
Intake Batch classification:
SentToClassification → PendingClassification → Classifying → Classified

Grouped Classified Batch:
Open → PendingWarehouseReceipt → WarehouseReceived → Stored
```

### 5.4 Inventory

```text
AwaitingPutaway → Available → Depleted
                       ↘ ReservedQuantity tăng khi Distribution được duyệt
```

### 5.5 Distribution

```text
PendingManagerApproval
  ├─→ Rejected
  └─→ ApprovedAwaitingWarehouse
          → ReadyForGhn
          → GhnBooked
          → trạng thái giao vận cập nhật từ GHN
```

## 6. Những điểm đã implement nhưng cần lưu ý

1. Một số entity vẫn có generic CRUD. Generic CRUD không bảo đảm đầy đủ business validation như operation service; UI nghiệp vụ nên gọi operation endpoint.
2. Donation Request enum có các trạng thái downstream, nhưng không phải mọi service đều cập nhật trực tiếp Donation Request ở từng bước; hành trình chi tiết chủ yếu dựa notification và quan hệ batch.
3. Ảnh item được UI bắt buộc, nhưng cần bổ sung validation server-side nếu đây là chứng từ bắt buộc.
4. Thuật toán tuyến hiện là heuristic theo địa chỉ/khu vực và cân bằng số đơn, chưa phải tối ưu VRP theo thời gian thực.
5. GHN hiện đồng bộ chủ động; webhook xác thực đầy đủ cần kiểm tra/bổ sung trước production.
6. Một số message tiếng Việt trong source đang có dấu hiệu lỗi encoding; không ảnh hưởng rule nhưng ảnh hưởng trải nghiệm và notification.

## 7. Business rules đề xuất cho vận hành thực tế

### 7.1 Quản trị và bảo mật

1. **Tách Admin và Manager**:
   - Admin quản lý role, quyền, cấu hình hệ thống, audit;
   - Manager quản lý nghiệp vụ trong các kho được phân quyền.
2. Manager cũng cần phạm vi kho, không mặc định xem/sửa toàn hệ thống.
3. Bổ sung refresh token, revoke token khi khóa tài khoản/đổi mật khẩu và session management.
4. Rate limit login, OTP, resend email và các API public.
5. Audit log bất biến cho create/update/delete/approve/issue/config, lưu before/after và actor.
6. Không lưu secret SMTP/GHN/DB trong appsettings được commit; dùng Azure App Settings/Key Vault.
7. Bổ sung permission chi tiết thay cho chỉ role, ví dụ `Warehouse.Layout.Edit`, `Distribution.Approve`.

### 7.2 Donation Request

1. Chỉ cho chọn ngày kho thực sự có lịch làm việc và còn capacity tiếp nhận.
2. Có time window thay vì chỉ ngày; DonorDropOff cũng nên chọn khoảng dự kiến hoặc check-in không hẹn.
3. Giới hạn số request/ngày/kho theo năng lực team và sức chứa.
4. Chống đơn spam/trùng theo donor, địa chỉ, thời gian và nội dung.
5. Cho donor sửa phương thức nhận nhưng phải revalidate kho/ngày và hủy assignment cũ an toàn.
6. Bổ sung trạng thái `DropOffOverdue`, `NoShow`, `PartiallyReceived`, `Closed`.
7. Lưu consent về dữ liệu cá nhân, ảnh và điều khoản quyên góp.
8. Có SLA: thời gian Manager phải phân công và staff phải liên hệ donor.

### 7.3 Ca, team và điều phối

1. Kiểm tra giờ hiện tại khi start/end Shift; chỉ cho start sớm/trễ trong tolerance được cấu hình.
2. Chấm công/check-in vị trí tại kho; ghi nhận người thực sự thao tác.
3. Giới hạn tải theo xe: khối lượng, thể tích, số điểm dừng và thời gian di chuyển.
4. Mỗi team gắn phương tiện, biển số, loại xe, capacity và tài xế đủ điều kiện.
5. Route optimization nên dùng tọa độ đã geocode, ma trận thời gian và traffic; lưu phiên bản route đã tối ưu.
6. Cho Manager khóa kế hoạch sau cutoff; thay đổi sau khóa phải có lý do/audit.
7. Có quy tắc xử lý staff nghỉ đột xuất, đổi ca, team thiếu người và bàn giao ca.
8. DonorDropOff chưa đến cuối ngày phải tự động chuyển overdue hoặc carry-forward có kiểm soát.
9. Không cho cùng người thuộc hai team overlap đã có; nên bổ sung kiểm tra ngày nghỉ/phép/chứng chỉ lái xe.

### 7.4 Tiếp nhận vật lý

1. Cân phải có sai số cho phép so với estimate; vượt ngưỡng yêu cầu supervisor duyệt.
2. Ảnh thực nhận tối thiểu và timestamp/location metadata phải được backend kiểm tra.
3. Hỗ trợ nhận một phần, từ chối một phần và ghi nguyên nhân theo mã chuẩn.
4. In/scan QR cho Intake Batch và niêm phong; seal number duy nhất.
5. Bàn giao Receiving → Classification phải có hai bên ký nhận và discrepancy workflow.
6. Không cho sửa ActualWeight sau bàn giao nếu không có reversal transaction.

### 7.5 Phân loại

1. Version hóa Category, Question, Answer và grading rule; item phải lưu version rule đã sử dụng.
2. Không thay đổi hồi tố kết quả cũ khi Manager sửa tiêu chí.
3. Bắt buộc ảnh server-side, kiểm tra MIME/kích thước/malware và lưu immutable URL.
4. Có QA sampling: một tỷ lệ item phải được supervisor kiểm tra lại.
5. Khi staff và QA khác nhãn, có dispute/reclassification nhưng giữ lịch sử.
6. Khối lượng grouped batch phải được cân thật, không chỉ phân bổ từ intake theo số item.
7. Batch được đóng theo ngày/ca/capacity; không để một grouped batch `Open` vô thời hạn.

### 7.6 Kho

1. Capacity cần quản lý cả kg và thể tích/số thùng/pallet.
2. Bổ sung lot/serial/QR, ngày nhập, tuổi tồn và FIFO/FEFO theo loại hàng.
3. Cycle count, stocktake, adjustment và approval cho chênh lệch tồn.
4. Transaction đã `Posted` không được sửa/xóa; sai phải tạo reversal transaction.
5. Reservation có thời hạn; request bị từ chối/hủy/quá hạn phải release reservation tự động.
6. Không cho quantity/weight/reserved âm bằng DB constraint và transaction isolation.
7. Optimistic concurrency/row version để tránh hai organization đặt cùng tồn đồng thời.
8. Location Blocked/Maintenance không chỉ chặn put-away mà còn có quy trình di dời hàng hiện có.
9. Cảnh báo capacity theo ngưỡng 70/85/95% và dự báo đầy kho.
10. Disposal cần quy trình phê duyệt và chứng từ tiêu hủy; Recycling cần chứng từ bàn giao.

### 7.7 Distribution và Organization

1. KYC/duyệt organization trước khi cho tạo request.
2. Quota theo tổ chức, khu vực, đối tượng hưởng lợi và chu kỳ thời gian.
3. Manager không được tự duyệt request do chính mình tạo/thao tác nếu áp dụng maker-checker.
4. Yêu cầu có ngày cần hàng, mức ưu tiên và bằng chứng chương trình từ thiện.
5. Partial approval: Manager được duyệt ít hơn requested quantity với lý do.
6. Organization chỉ được hủy trước khi xuất kho; sau reserve phải release atomically.
7. Proof of delivery, người nhận ký/ảnh và xác nhận Organization đã nhận.
8. Đơn thất bại/hoàn hàng phải tạo inbound return transaction và phục hồi inventory đúng location kiểm tra.
9. GHN webhook phải xác thực chữ ký/token, idempotent và lưu raw payload phục vụ audit.
10. Retry có backoff cho GHN; không tạo trùng vận đơn khi client retry.
11. Phí vận chuyển, COD/payment party và đối soát phải được lưu thành dữ liệu nghiệp vụ.

### 7.8 Notification và SLA

1. Dùng outbox pattern để thay đổi DB và notification không bị lệch khi lỗi giữa chừng.
2. Notification cần idempotency key để tránh duplicate.
3. Cho người dùng cấu hình kênh email/in-app và loại thông báo.
4. Escalation tự động cho đơn quá SLA, batch chưa nhận, tồn lâu và shipment đứng trạng thái.
5. Template notification version hóa, đa ngôn ngữ, không ghép chuỗi trực tiếp trong service.

### 7.9 Dữ liệu và báo cáo

1. Chuẩn hóa timezone: lưu UTC, hiển thị Asia/Ho_Chi_Minh; ngày nghiệp vụ dùng local date có chủ đích.
2. DB constraints cho mã duy nhất, số lượng/khối lượng không âm và quan hệ status hợp lệ.
3. Báo cáo cần snapshot hoặc event ledger thay vì suy ra hoàn toàn từ trạng thái hiện tại.
4. KPI đề xuất: lead time phân công, pickup success, no-show, phân loại/item-hour, inventory aging, utilization, fulfillment rate, kg theo hướng xử lý, tỷ lệ chênh lệch cân.
5. Chính sách retention/anonymization dữ liệu donor sau thời hạn pháp lý.

## 8. Ưu tiên triển khai đề xuất

### P0 – Trước khi production

- DB constraints và concurrency cho tồn/reservation.
- Audit log và reversal transaction.
- Version hóa tiêu chí phân loại.
- GHN idempotency, webhook authentication và retry.
- Manager scope theo kho và revoke token khi khóa tài khoản.
- Outbox/idempotency cho notification.

### P1 – Vận hành ổn định

- SLA/escalation, no-show/drop-off overdue.
- QA phân loại và discrepancy workflow.
- Vehicle capacity và route optimization thực tế.
- Cycle count/stock adjustment.
- Organization KYC, quota và proof of delivery.

### P2 – Tối ưu và mở rộng

- Workflow Recycling Organization hoàn chỉnh.
- Disposal compliance/chứng từ.
- Dự báo capacity, inventory aging và BI/KPI nâng cao.
- Email/push preference và template đa ngôn ngữ.

## 9. Kết luận mức độ hoàn thiện

Luồng chính `Donor → Receiving → Classification → Warehouse → Charity Distribution → GHN` đã có các operation service và state transition tương đối đầy đủ. Các rule mạnh nhất hiện nằm ở Receiving, Classification, Warehouse và Distribution services. Trước khi dùng production, ưu tiên lớn nhất không phải thêm màn hình mà là tăng tính nhất quán dữ liệu, audit, concurrency, version hóa tiêu chí, shipment idempotency và phân quyền theo phạm vi kho.
