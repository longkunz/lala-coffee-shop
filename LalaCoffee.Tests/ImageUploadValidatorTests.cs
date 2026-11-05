using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using LalaCoffee.Controllers;
using LalaCoffee.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class ImageUploadValidatorTests
    {
        private class FakeHttpPostedFileBase : HttpPostedFileBase
        {
            private readonly string _fileName;
            private readonly string _contentType;
            private readonly int _contentLength;

            public FakeHttpPostedFileBase(string fileName, string contentType, int contentLength)
            {
                _fileName = fileName;
                _contentType = contentType;
                _contentLength = contentLength;
            }

            public override string FileName => _fileName;
            public override string ContentType => _contentType;
            public override int ContentLength => _contentLength;
        }

        [TestMethod]
        [DataRow("avatar.jpg")]
        [DataRow("photo.jpeg")]
        [DataRow("IMAGE.PNG")]
        [DataRow("animation.gif")]
        [DataRow("banner.webp")]
        [DataRow("test.avatar.jpg")]
        public void IsValidExtension_WithAllowedExtensions_ReturnsTrue(string fileName)
        {
            bool result = ImageUploadValidator.IsValidExtension(fileName);
            Assert.IsTrue(result, $"Expected {fileName} to be valid extension.");
        }

        [TestMethod]
        [DataRow("shell.aspx")]
        [DataRow("malware.exe")]
        [DataRow("script.sh")]
        [DataRow("backdoor.php")]
        [DataRow("web.config")]
        [DataRow("payload.cshtml")]
        [DataRow("library.dll")]
        [DataRow("batch.bat")]
        [DataRow("hack.js")]
        [DataRow("test.svg")]
        public void IsValidExtension_WithInvalidExtensions_ReturnsFalse(string fileName)
        {
            bool result = ImageUploadValidator.IsValidExtension(fileName);
            Assert.IsFalse(result, $"Expected {fileName} to be invalid extension.");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow(null)]
        [DataRow("fileWithoutExtension")]
        [DataRow("test.")]
        public void IsValidExtension_WithMissingOrEmptyExtension_ReturnsFalse(string fileName)
        {
            bool result = ImageUploadValidator.IsValidExtension(fileName);
            Assert.IsFalse(result, $"Expected '{fileName}' to be invalid extension.");
        }

        [TestMethod]
        [DataRow("image/jpeg")]
        [DataRow("image/pjpeg")]
        [DataRow("image/png")]
        [DataRow("image/x-png")]
        [DataRow("image/gif")]
        [DataRow("image/webp")]
        public void IsValidMimeType_WithAllowedMimeTypes_ReturnsTrue(string mimeType)
        {
            bool result = ImageUploadValidator.IsValidMimeType(mimeType);
            Assert.IsTrue(result, $"Expected {mimeType} to be valid MIME type.");
        }

        [TestMethod]
        [DataRow("application/x-msdownload")]
        [DataRow("text/plain")]
        [DataRow("text/html")]
        [DataRow("application/octet-stream")]
        [DataRow("")]
        [DataRow(null)]
        public void IsValidMimeType_WithInvalidMimeTypes_ReturnsFalse(string mimeType)
        {
            bool result = ImageUploadValidator.IsValidMimeType(mimeType);
            Assert.IsFalse(result, $"Expected {mimeType} to be invalid MIME type.");
        }

        [TestMethod]
        [DataRow(1L)]
        [DataRow(1024L)]
        [DataRow(1024L * 1024L)]
        [DataRow(5L * 1024L * 1024L)] // Exactly 5MB
        public void IsValidSize_WithValidSizes_ReturnsTrue(long size)
        {
            bool result = ImageUploadValidator.IsValidSize(size);
            Assert.IsTrue(result, $"Expected size {size} to be valid.");
        }

        [TestMethod]
        [DataRow(5L * 1024L * 1024L + 1L)] // 5MB + 1 byte
        [DataRow(10L * 1024L * 1024L)]     // 10MB
        public void IsValidSize_WithOversizedFiles_ReturnsFalse(long size)
        {
            bool result = ImageUploadValidator.IsValidSize(size);
            Assert.IsFalse(result, $"Expected size {size} to be invalid (oversized).");
        }

        [TestMethod]
        [DataRow(0L)]
        [DataRow(-1L)]
        [DataRow(-100L)]
        public void IsValidSize_WithEmptyOrNegativeSize_ReturnsFalse(long size)
        {
            bool result = ImageUploadValidator.IsValidSize(size);
            Assert.IsFalse(result, $"Expected size {size} to be invalid (empty or negative).");
        }

        [TestMethod]
        public void GenerateSafeFileName_WithValidFileName_ReturnsGuidWithOriginalLowerExtension()
        {
            string originalFileName = "MyPhoto.JPEG";
            string safeName = ImageUploadValidator.GenerateSafeFileName(originalFileName);

            Assert.IsNotNull(safeName);
            Assert.IsTrue(safeName.EndsWith(".jpeg", StringComparison.Ordinal), "Extension must be lowercased .jpeg");

            string nameWithoutExt = Path.GetFileNameWithoutExtension(safeName);
            Assert.AreEqual(32, nameWithoutExt.Length, "Expected 32-char Guid (N format)");
            Assert.IsTrue(Guid.TryParse(nameWithoutExt, out _), "Safe name prefix must be a valid Guid");
        }

        [TestMethod]
        public void GenerateSafeFileName_ReturnsUniqueNamesAcrossMultipleCalls()
        {
            string name1 = ImageUploadValidator.GenerateSafeFileName("test.png");
            string name2 = ImageUploadValidator.GenerateSafeFileName("test.png");

            Assert.AreNotEqual(name1, name2);
        }

        [TestMethod]
        public void GenerateSafeFileName_WithNullOrEmpty_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => ImageUploadValidator.GenerateSafeFileName(""));
            Assert.Throws<ArgumentException>(() => ImageUploadValidator.GenerateSafeFileName(null));
        }

        [TestMethod]
        public void ImageController_HasAuthorizeAdminAttribute()
        {
            var controllerType = typeof(ImageController);
            var authorizeAttr = controllerType.GetCustomAttribute<AuthorizeAttribute>();

            Assert.IsNotNull(authorizeAttr, "ImageController should have [Authorize] attribute");
            Assert.AreEqual("Admin", authorizeAttr.Roles, "ImageController Authorize attribute should require 'Admin' role");
        }

        [TestMethod]
        public void ProcessUpload_WithNullFile_ReturnsBadRequest()
        {
            using (var controller = new ImageController())
            {
                var result = controller.ProcessUpload(null) as HttpStatusCodeResult;
                Assert.IsNotNull(result);
                Assert.AreEqual(400, result.StatusCode);
            }
        }

        [TestMethod]
        public void ProcessUpload_WithInvalidMimeType_ReturnsBadRequest()
        {
            using (var controller = new ImageController())
            {
                var file = new FakeHttpPostedFileBase("photo.png", "application/x-msdownload", 1024);
                var result = controller.ProcessUpload(file) as HttpStatusCodeResult;
                Assert.IsNotNull(result);
                Assert.AreEqual(400, result.StatusCode);
                Assert.AreEqual("Định dạng MIME không hợp lệ.", result.StatusDescription);
            }
        }

        [TestMethod]
        public void ProcessUpload_WithInvalidExtension_ReturnsBadRequest()
        {
            using (var controller = new ImageController())
            {
                var file = new FakeHttpPostedFileBase("photo.exe", "image/png", 1024);
                var result = controller.ProcessUpload(file) as HttpStatusCodeResult;
                Assert.IsNotNull(result);
                Assert.AreEqual(400, result.StatusCode);
                Assert.AreEqual("Định dạng file không được phép. Chỉ chấp nhận JPG, JPEG, PNG, WEBP, GIF.", result.StatusDescription);
            }
        }

        [TestMethod]
        public void ProcessUpload_WithOversizedFile_ReturnsBadRequest()
        {
            using (var controller = new ImageController())
            {
                var file = new FakeHttpPostedFileBase("photo.png", "image/png", 6 * 1024 * 1024);
                var result = controller.ProcessUpload(file) as HttpStatusCodeResult;
                Assert.IsNotNull(result);
                Assert.AreEqual(400, result.StatusCode);
                Assert.AreEqual("Dung lượng file vượt quá giới hạn cho phép (tối đa 5MB).", result.StatusDescription);
            }
        }
    }
}
