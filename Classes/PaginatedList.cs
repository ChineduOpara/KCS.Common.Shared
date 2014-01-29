using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    public interface IPaginatedList
    {
        ushort PageIndex { get; set; }
        ushort PreviousPage { get; }
        ushort NextPage { get; }

        ushort PageSize { get; set; }
        ushort TotalCount { get; set; }
        ushort TotalPages { get; set; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }

        ushort[] PageSizes { get; set; }

        ushort MaxPageButtonDisplay { get; set; }
        ushort StartPage { get; set; }
        ushort EndPage { get; set; }
        bool ShiftPageNumbers { get; set; }
    }

    public class PaginatedList<T> : List<T>, IPaginatedList
    {
        public ushort PageIndex { get; set; }
        public ushort PreviousPage { get; set; }
        public ushort NextPage { get; set;  }

        public ushort PageSize { get; set; }
        public ushort TotalCount { get; set; }
        public ushort TotalPages { get; set; }

        public ushort[] PageSizes { get; set; }

        public ushort MaxPageButtonDisplay { get; set; }
        public ushort StartPage { get; set; }
        public ushort EndPage { get; set; }
        public bool ShiftPageNumbers { get; set; }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 0);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex + 1 < TotalPages);
            }
        }

        public PaginatedList(IQueryable<T> source, ushort pageIndex = 0, ushort pageSize = 10)
        {
            MaxPageButtonDisplay = 10;

            PageSizes = new ushort[] { 5, 10, 25, 50 };

            TotalCount = (ushort)source.Count();
            PageIndex = pageIndex;
            PageSize = pageSize;            

            // Correction of PageSize
            if (PageSize < PageSizes.First())
            {
                PageSize = PageSizes.First();
            }

            if (PageSize > PageSizes.Last())
            {
                PageSize = PageSizes.Last();
            }

            TotalPages = (ushort)Math.Ceiling(TotalCount / (double)PageSize);

            // Correction of PageIndex
            if (TotalPages > 0 && PageIndex > TotalPages - 1)
            {
                PageIndex = Convert.ToUInt16(TotalPages - 1);
            }            

            int skip = Convert.ToInt32(PageIndex * PageSize);
            this.AddRange(source.Skip(skip).Take(PageSize));

            if (HasPreviousPage)
            {
                PreviousPage = Convert.ToUInt16(PageIndex - 1);
            }
            if (HasNextPage)
            {
                NextPage = Convert.ToUInt16(PageIndex + 1);
            }

            // Set the Page-shifting properties
            EndPage = TotalPages;
            if (TotalPages > MaxPageButtonDisplay)
            {
                ShiftPageNumbers = true;

                if (PageIndex < MaxPageButtonDisplay - 1)
                {
                    EndPage = MaxPageButtonDisplay;
                }
                else
                {
                    if (PageIndex < TotalPages - 1)
                    {
                        EndPage = (ushort)(PageIndex + 2);
                    }
                    else
                    {
                        EndPage = TotalPages;
                    }
                    StartPage = (ushort) (EndPage - MaxPageButtonDisplay);
                }
            }
        }        
    }
}
