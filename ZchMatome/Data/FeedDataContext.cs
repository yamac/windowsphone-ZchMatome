using System.Data.Linq;
using Microsoft.Phone.Data.Linq;

namespace ZchMatome.Data
{
    public class FeedDataContext : DataContext
    {
        private const string DBConnectionString = "Data Source=isostore:/Feed.sdf";
        private const int DBSchemaVersion = 1;

        public FeedDataContext()
            : base(DBConnectionString)
        {
            if (DatabaseExists() == false)
            {
                System.Diagnostics.Debug.WriteLine("Create database");
                CreateDatabase();
                DatabaseSchemaUpdater updater = this.CreateDatabaseSchemaUpdater();
                updater.DatabaseSchemaVersion = DBSchemaVersion;
                updater.Execute();
            }
            else
            {
                DatabaseSchemaUpdater updater = this.CreateDatabaseSchemaUpdater();
                int version = updater.DatabaseSchemaVersion;
                if (version == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Update database from " + version + " to " + DBSchemaVersion);
                    updater.DatabaseSchemaVersion = DBSchemaVersion;
                    updater.Execute();

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No database changes");
                }
            }
        }

        public Table<FeedGroup> FeedGroups;
        public Table<FeedChannel> FeedChannels;
    }
}
