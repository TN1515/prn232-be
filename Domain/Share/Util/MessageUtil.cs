using Domain.Payload.Base;

namespace Domain.Shares.Util
{
    public static class MessageUtil
    {
        public static string ToMarkdown(LogMessage message)
        {
            return
                $"""
                🟢 **{message.Action}**

                👤 **Người dùng:** {message.UserName}
                🆔 **Mã SP:** `{message.ID}`
                🏷️ **Tên SP:** *{message.Name}*
                🕒 **Thời gian:** {message.Timestamp:dd/MM/yyyy HH:mm:ss}
                """;
        }
        public static string BuildSubmittedEmail(string fullName, string jobTitle)
        {
            return BaseEmailTemplate("THÔNG BÁO ỨNG TUYỂN THÀNH CÔNG", fullName, $@"
            <p>Chúng tôi đã nhận được đơn ứng tuyển của bạn cho vị trí <strong>{jobTitle}</strong>.</p>
            <p>Hồ sơ đang được chuyển đến bộ phận tuyển dụng để đánh giá. Chúng tôi sẽ liên hệ nếu bạn phù hợp với vị trí.</p>
            <p>Cảm ơn bạn đã quan tâm đến cơ hội tại <strong>Công Ty Cổ Phần IIT</strong>.</p>
        ");
        }

        public static string BuildUnderReviewEmail(string fullName, string jobTitle)
        {
            return BaseEmailTemplate("HỒ SƠ ĐANG ĐƯỢC XEM XÉT", fullName, $@"
            <p>Chúng tôi xác nhận rằng hồ sơ của bạn cho vị trí <strong>{jobTitle}</strong> đang được xem xét kỹ lưỡng bởi bộ phận chuyên môn.</p>
            <p>Chúng tôi sẽ sớm phản hồi nếu bạn đáp ứng được yêu cầu công việc.</p>
        ");
        }

        public static string BuildInterviewScheduledEmail(string fullName, string jobTitle, DateOnly date, TimeOnly interviewTime, string location)
        {
            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <style>
    body {{
      font-family: 'Segoe UI', Tahoma, sans-serif;
      background-color: #f4f6f8;
      margin: 0;
      padding: 0;
    }}
    .wrapper {{
      width: 100%;
      padding: 30px 0;
    }}
    .email-card {{
      max-width: 600px;
      margin: auto;
      background-color: #ffffff;
      border: 1px solid #e0e6ed;
      border-radius: 6px;
      overflow: hidden;
      box-shadow: 0 2px 6px rgba(0,0,0,0.05);
    }}
    .header {{
      background-color: #1a3c66;
      padding: 25px;
      text-align: center;
      color: #ffffff;
    }}
    .header img {{
      height: 50px;
      margin-bottom: 10px;
    }}
    .header h2 {{
      margin: 0;
      font-size: 20px;
    }}
    .body {{
      padding: 30px;
      font-size: 15px;
      color: #333333;
    }}
    .body p {{
      margin-bottom: 16px;
    }}
    .info-block {{
      background-color: #f0f2f7;
      padding: 20px;
      border-radius: 5px;
      margin: 20px 0;
      line-height: 1.6;
    }}
    .info-block strong {{
      display: inline-block;
      width: 90px;
    }}
    .footer {{
      background-color: #f2f4f8;
      text-align: center;
      padding: 18px;
      font-size: 13px;
      color: #888;
    }}
  </style>
</head>
<body>
  <div class='wrapper'>
    <div class='email-card'>
      <div class='header'>
        <img src='https://www.iit.vn/assets/iit/logo.png' alt='IIT Logo'>
        <h2>THƯ MỜI PHỎNG VẤN</h2>
      </div>
      <div class='body'>
        <p>Xin chào <strong>{fullName}</strong>,</p>
        <p>Chúng tôi trân trọng mời bạn tham gia buổi phỏng vấn cho vị trí <strong>{jobTitle}</strong> tại Công Ty Cổ Phần IIT.</p>

        <div class='info-block'>
          <p><strong>Ngày:</strong> {date:dddd, dd/MM/yyyy}</p>
          <p><strong>Thời gian:</strong> {interviewTime}</p>
          <p><strong>Địa điểm:</strong> {location}</p>
        </div>

        <p>Vui lòng xác nhận sự tham gia qua email này để chúng tôi sắp xếp buổi phỏng vấn phù hợp.</p>
        <p>Chúc bạn có một buổi phỏng vấn thành công!</p>

        <p>Trân trọng,<br/>Phòng Tuyển dụng - IIT</p>
      </div>
      <div class='footer'>
        © 2025 IIT Corporation. All rights reserved.
      </div>
    </div>
  </div>
</body>
</html>";
        }

        public static string BuildInterviewedEmail(string fullName, string jobTitle)
        {
            return BaseEmailTemplate("CẢM ƠN BẠN ĐÃ THAM GIA PHỎNG VẤN", fullName, $@"
            <p>Chúng tôi chân thành cảm ơn bạn đã tham gia buổi phỏng vấn cho vị trí <strong>{jobTitle}</strong>.</p>
            <p>Kết quả sẽ được thông báo trong thời gian sớm nhất sau khi hội đồng tuyển dụng đánh giá.</p>
        ");
        }

        public static string BuildOfferedEmail(string fullName, string jobTitle)
        {
            return BaseEmailTemplate("CHÚC MỪNG! BẠN ĐƯỢC NHẬN VIỆC", fullName, $@"
            <p>Bạn đã vượt qua các vòng tuyển chọn và được đề xuất nhận việc cho vị trí <strong>{jobTitle}</strong>.</p>
            <p>Thông tin chi tiết về hợp đồng sẽ được gửi trong thời gian tới.</p>
            <p>Chúng tôi rất mong được chào đón bạn gia nhập <strong>IIT</strong>.</p>
        ");
        }

        public static string BuildRejectedEmail(string fullName, string jobTitle)
        {
            return BaseEmailTemplate("KẾT QUẢ ỨNG TUYỂN", fullName, $@"
            <p>Sau quá trình xem xét, chúng tôi rất tiếc thông báo rằng bạn chưa phù hợp với vị trí <strong>{jobTitle}</strong> tại thời điểm hiện tại.</p>
            <p>Hy vọng được đồng hành cùng bạn trong các cơ hội khác tại IIT.</p>
        ");
        }

        private static string BaseEmailTemplate(string title, string fullName, string innerHtml)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
  <style>
    body {{
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      background-color: #f2f6fc;
      margin: 0;
      padding: 0;
    }}
    .container {{
      max-width: 600px;
      margin: 40px auto;
      background-color: #ffffff;
      border-radius: 12px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
      overflow: hidden;
    }}
    .header {{
      background-color: #9FB6CD;
      padding: 30px;
      text-align: center;
      color: white;
    }}
    .header img {{
      height: 60px;
      margin-bottom: 10px;
    }}
    .content {{
      padding: 30px;
      font-size: 16px;
      color: #333333;
    }}
    .content p {{
      margin: 0 0 15px 0;
    }}
    .footer {{
      background-color: #f1f4f9;
      padding: 20px;
      text-align: center;
      font-size: 13px;
      color: #888;
    }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='header'>
      <img src='https://www.iit.vn/assets/iit/logo.png' alt='IIT Logo'>
      <h2>{title}</h2>
    </div>
    <div class='content'>
      <p>Chào <strong>{fullName}</strong>,</p>
      {innerHtml}
      <p>Trân trọng,<br>Phòng Tuyển dụng - IIT</p>
    </div>
    <div class='footer'>
      © 2025 IIT Corporation. All rights reserved.
    </div>
  </div>
</body>
</html>";
        }
        public static string GenerateForgotPasswordEmail(string code)
        {
            return $@"
    <html>
      <head>
        <style>
          body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
          }}
          .container {{
            max-width: 600px;
            margin: 40px auto;
            background: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            padding: 30px;
          }}
          h2 {{
            color: #333333;
          }}
          .code-box {{
            margin: 20px 0;
            padding: 15px;
            text-align: center;
            font-size: 24px;
            font-weight: bold;
            color: #ffffff;
            background: #007bff;
            border-radius: 6px;
            letter-spacing: 4px;
          }}
          .footer {{
            margin-top: 30px;
            font-size: 12px;
            color: #777777;
            text-align: center;
          }}
        </style>
      </head>
      <body>
        <div class='container'>
          <h2>Khôi phục mật khẩu</h2>
          <p>Xin chào,</p>
          <p>Bạn đã yêu cầu khôi phục mật khẩu. Vui lòng sử dụng mã xác nhận bên dưới để tiếp tục:</p>
          <div class='code-box'>{code}</div>
          <p>Mã này có hiệu lực trong <b>5 phút</b>. Nếu bạn không yêu cầu khôi phục, vui lòng bỏ qua email này.</p>
          <div class='footer'>
            &copy; {DateTime.Now.Year} - Hệ thống của chúng tôi
          </div>
        </div>
      </body>
    </html>";
        }

        public static string GenerateInactiveUserWarningEmail(
    string userName,
    string email,
    string fullName,
    string phone,
    string gender,
    DateOnly? dob,
    string address,
    string avatarUrl,
    DateTime createdDate,
    DateTime? modifyDate,
    string role,
    DateTime deadline)
        {
            return $@"
<html>
  <head>
    <style>
      body {{
        font-family: Arial, sans-serif;
        background-color: #f4f4f4;
        margin: 0;
        padding: 0;
      }}
      .container {{
        max-width: 650px;
        margin: 40px auto;
        background: #ffffff;
        border-radius: 8px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        padding: 30px;
      }}
      h2 {{
        color: #333333;
      }}
      .info-table {{
        width: 100%;
        border-collapse: collapse;
        margin-top: 15px;
      }}
      .info-table td {{
        padding: 8px 10px;
        border-bottom: 1px solid #eee;
      }}
      .info-label {{
        font-weight: bold;
        color: #555;
        width: 30%;
      }}
      .info-value {{
        color: #333;
      }}
      .warning-box {{
        margin: 25px 0;
        padding: 15px;
        text-align: center;
        font-size: 17px;
        font-weight: bold;
        color: #ffffff;
        background: #dc3545;
        border-radius: 6px;
      }}
      .avatar {{
        display: block;
        width: 90px;
        height: 90px;
        border-radius: 50%;
        object-fit: cover;
        margin: 10px auto;
        border: 3px solid #007bff;
      }}
      .footer {{
        margin-top: 30px;
        font-size: 12px;
        color: #777777;
        text-align: center;
      }}
    </style>
  </head>
  <body>
    <div class='container'>
      <h2>Thông báo quan trọng về tài khoản shop của bạn</h2>
      <p>Xin chào <b>{fullName}</b>,</p>
      <p>Tài khoản shop của bạn hiện chưa được thiết lập đầy đủ thông tin.</p>

      {(string.IsNullOrEmpty(avatarUrl) ? "" : $"<img src='{avatarUrl}' alt='Avatar' class='avatar' />")}

      <table class='info-table'>
        <tr><td class='info-label'>Tên đăng nhập:</td><td class='info-value'>{userName}</td></tr>
        <tr><td class='info-label'>Email:</td><td class='info-value'>{email}</td></tr>
        <tr><td class='info-label'>Họ tên:</td><td class='info-value'>{fullName}</td></tr>
        <tr><td class='info-label'>Số điện thoại:</td><td class='info-value'>{phone}</td></tr>
        <tr><td class='info-label'>Giới tính:</td><td class='info-value'>{(string.IsNullOrEmpty(gender) ? "Chưa cập nhật" : gender)}</td></tr>
        <tr><td class='info-label'>Ngày sinh:</td><td class='info-value'>{(dob.HasValue ? dob.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật")}</td></tr>
        <tr><td class='info-label'>Địa chỉ:</td><td class='info-value'>{(string.IsNullOrEmpty(address) ? "Chưa cập nhật" : address)}</td></tr>
        <tr><td class='info-label'>Ngày tạo:</td><td class='info-value'>{createdDate:HH:mm dd/MM/yyyy}</td></tr>
        <tr><td class='info-label'>Cập nhật gần nhất:</td><td class='info-value'>{(modifyDate.HasValue ? modifyDate.Value.ToString("HH:mm dd/MM/yyyy") : "Chưa cập nhật")}</td></tr>
        <tr><td class='info-label'>Vai trò:</td><td class='info-value'>{role}</td></tr>
      </table>

      <div class='warning-box'>
        Nếu bạn không cập nhật thông tin trong vòng 1 giờ (trước {deadline:HH:mm - dd/MM/yyyy}),<br/>
        tài khoản của bạn sẽ bị <b>xóa vĩnh viễn</b> khỏi hệ thống.
      </div>

      <p>Vui lòng đăng nhập vào hệ thống để hoàn tất thiết lập thông tin tài khoản.</p>

      <div class='footer'>
        &copy; {DateTime.Now.Year} - Hệ thống Quản Lý Shop
      </div>
    </div>
  </body>
</html>";
        }


    }


}

