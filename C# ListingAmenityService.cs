using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models;
using Sabio.Models.Domain.ListAmenity;
using Sabio.Models.Requests.ListingAmenity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabio.Services
{
    public class ListingAmenityService : IListingAmenityService
    {
        private IDataProvider _data = null;

        public ListingAmenityService(IDataProvider data)
        {
            _data = data;
        }

        public int AddListingAmenity(ListingAmenityAddRequest model)
        {
            string procName = "[dbo].[ListingAmenities_Batch_Insert]";

            int id = 0;
            DataTable newAmenities = null;
            if (model.Amenities != null)
            {
                newAmenities = MapListingAmenityToTable(model.Amenities );
            }

            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonParams(model, col);
                col.AddWithValue("@batchAmenities", newAmenities);


            
            });

            return id;

        }

        public void UpdateListingAmenity(ListingAmenityUpdateRequest model)
        {
            string procName = "[dbo].[ListingAmenities_Update]";
            DataTable newAmenities = null;
            if (model.Amenities != null)
            {
                newAmenities = MapListingAmenityToTable(model.Amenities);
            }

            _data.ExecuteNonQuery(procName, inputParamMapper: (SqlParameterCollection col) =>
           {


               AddCommonParams(model, col);
               col.AddWithValue("@ListingId", model.ListingId);

           }, returnParameters: null);

        }

        public Paged<ListingAmenity> Pagination(int pageIndex, int pageSize)

        {
            Paged<ListingAmenity> pagedList = null;

            List<ListingAmenity> listingAmenityList = null;
            int totalCount = 0;

            string procName = "[dbo].[ListingAmenities_SelectAll_Paginated]";

            _data.ExecuteCmd(procName, delegate (SqlParameterCollection col)

            {
                col.AddWithValue("@PageIndex", pageIndex);
                col.AddWithValue("PageSize", pageSize);

            }, delegate (IDataReader reader, short set)
            {
                int StartingIndex = 0;
                ListingAmenity listingAmenity = SingleListingAmenityMapper(reader, ref StartingIndex);
                if (totalCount == 0)
                {
                    totalCount = reader.GetSafeInt32(StartingIndex++);
                }
                if (listingAmenityList == null)
                {
                    listingAmenityList = new List<ListingAmenity>();
                }

                listingAmenityList.Add(listingAmenity);
            });

            if (listingAmenityList != null)
            {
                pagedList = new Paged<ListingAmenity>(listingAmenityList, pageIndex, pageSize, totalCount);
            }

            return pagedList;


        }


        private static ListingAmenity SingleListingAmenityMapper(IDataReader reader, ref int index)
        {
            ListingAmenity model = new ListingAmenity();

            model.ListingId = reader.GetSafeInt32(index++);
            model.AmenityId = reader.GetSafeInt32(index++);

            return model;
        }

        private static DataTable MapListingAmenityToTable(List<string> amenities)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            foreach (string amenity in amenities)
            {
                DataRow row = table.NewRow();
                int startingIndex = 0;
                row.SetField(startingIndex++, amenity);
                table.Rows.Add(row);
            }
            return table;
        }
        private static void AddCommonParams(ListingAmenityAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@ListingId", model.ListingId);
            //col.AddWithValue("@AmenityId", model.AmenityId);

        }


    }
}
