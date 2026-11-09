using System;
using System.Collections.Generic;
using System.IO;

namespace LalaCoffee.Helpers
{
	public static class ImageUploadValidator
	{
		public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

		public static readonly HashSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			".jpg",
			".jpeg",
			".png",
			".gif",
			".webp"
		};

		public static readonly HashSet<string> AllowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"image/jpeg",
			"image/pjpeg",
			"image/png",
			"image/x-png",
			"image/gif",
			"image/webp"
		};

		public static bool IsValidExtension(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				return false;
			}

			string extension = Path.GetExtension(fileName);
			if (string.IsNullOrEmpty(extension))
			{
				return false;
			}

			return AllowedExtensions.Contains(extension);
		}

		public static bool IsValidMimeType(string contentType)
		{
			if (string.IsNullOrWhiteSpace(contentType))
			{
				return false;
			}

			return AllowedMimeTypes.Contains(contentType.Trim());
		}

		public static bool IsValidSize(long contentLength)
		{
			return contentLength > 0 && contentLength <= MaxFileSizeBytes;
		}

		public static string GenerateSafeFileName(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
			}

			string extension = Path.GetExtension(fileName).ToLowerInvariant();
			return $"{Guid.NewGuid():N}{extension}";
		}
	}
}
