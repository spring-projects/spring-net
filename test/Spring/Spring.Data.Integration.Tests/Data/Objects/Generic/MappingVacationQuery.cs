

using System.Data;
#if NET_2_0 || NET_3_0
namespace Spring.Data.Objects.Generic
{
    public class MappingVacationQuery : MappingAdoQuery
    {
        protected override T MapRow<T>(IDataReader reader, int num)
        {
            throw new System.NotImplementedException();
        }

        private VacationRowMapper<Vacation> vacationRowMapper = new VacationRowMapper<Vacation>();
    }
}
#endif