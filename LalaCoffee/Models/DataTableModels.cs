using System;
using System.Collections.Generic;
using System.Web;

namespace LalaCoffee.Models
{
	public class DataTableRequest
	{
		public int Draw { get; set; }
		public int Start { get; set; }
		public int Length { get; set; }
		public string SearchValue { get; set; }
		public int SortColumnIndex { get; set; }
		public string SortDirection { get; set; }

		public static DataTableRequest Parse(HttpRequestBase request)
		{
			if (request == null)
			{
				return new DataTableRequest
				{
					Draw = 1,
					Start = 0,
					Length = 10,
					SearchValue = string.Empty,
					SortColumnIndex = 0,
					SortDirection = "asc"
				};
			}

			int draw;
			if (!int.TryParse(request["draw"], out draw))
			{
				draw = 1;
			}

			int start;
			if (!int.TryParse(request["start"], out start) || start < 0)
			{
				start = 0;
			}

			int length;
			if (!int.TryParse(request["length"], out length) || length <= 0)
			{
				length = 10;
			}

			if (length > 500)
			{
				length = 500;
			}

			string searchValue = request["search[value]"];
			if (string.IsNullOrWhiteSpace(searchValue))
			{
				searchValue = request["search"];
			}

			int sortColumnIndex;
			if (!int.TryParse(request["order[0][column]"], out sortColumnIndex) || sortColumnIndex < 0)
			{
				sortColumnIndex = 0;
			}

			string sortDirection = request["order[0][dir]"] ?? "asc";
			if (!string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase))
			{
				sortDirection = "asc";
			}
			else
			{
				sortDirection = "desc";
			}

			return new DataTableRequest
			{
				Draw = draw,
				Start = start,
				Length = length,
				SearchValue = searchValue != null ? searchValue.Trim() : string.Empty,
				SortColumnIndex = sortColumnIndex,
				SortDirection = sortDirection
			};
		}
	}

	public class DataTableResponse<T>
	{
		public int draw { get; set; }
		public int recordsTotal { get; set; }
		public int recordsFiltered { get; set; }
		public IEnumerable<T> data { get; set; }
	}
}
