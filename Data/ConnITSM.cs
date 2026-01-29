using Microsoft.EntityFrameworkCore;

namespace TP_ITSM.Data
{
    public class ConnITSM : DbContext
    {
        public ConnITSM(DbContextOptions<ConnITSM> options)
                : base(options)
        {
        }
    }
}
