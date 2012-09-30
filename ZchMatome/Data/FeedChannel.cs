using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Runtime.Serialization;
using SimpleMvvmToolkit;

namespace ZchMatome.Data
{
    [DataContract]
    [Table]
    public class FeedChannel : ModelBase<FeedChannel>
    {
        [Column(IsVersion = true)]
#pragma warning disable
        private Binary version;
#pragma warning restore

        private int _Id;
        [DataMember(Name = "id")]
        [Column(IsPrimaryKey = true, IsDbGenerated = false, CanBeNull = false)]
        public int Id
        {
            get { return _Id; }
            set
            {
                if (_Id == value) return;
                _Id = value;
                NotifyPropertyChanged(m => Id);
            }
        }

        private int? _OrderNum;
        [DataMember(Name = "order_num")]
        [Column]
        public int? OrderNum
        {
            get { return _OrderNum; }
            set
            {
                if (_OrderNum == value) return;
                _OrderNum = value;
                NotifyPropertyChanged(m => OrderNum);
            }
        }

        private int _FeedGroupId;
        [DataMember(Name = "feed_group_id")]
        [Column]
        public int FeedGroupId
        {
            get { return _FeedGroupId; }
            set
            {
                if (_FeedGroupId == value) return;
                _FeedGroupId = value;
                NotifyPropertyChanged(m => FeedGroupId);
            }
        }

        private EntityRef<FeedGroup> _FeedGroup;
        [Association(Storage = "_FeedGroup", ThisKey = "FeedGroupId", OtherKey = "Id")]
        public FeedGroup FeedGroup
        {
            get { return _FeedGroup.Entity; }
            set { _FeedGroup.Entity = value; }
        }

        private string _FeedLink;
        [DataMember(Name = "feed_link")]
        [Column]
        public string FeedLink
        {
            get { return _FeedLink; }
            set
            {
                if (_FeedLink == value) return;
                _FeedLink = value;
                NotifyPropertyChanged(m => FeedLink);
            }
        }

        private string _AuthorName;
        [DataMember(Name = "author_name")]
        [Column]
        public string AuthorName
        {
            get { return _AuthorName; }
            set
            {
                if (_AuthorName == value) return;
                _AuthorName = value;
                NotifyPropertyChanged(m => AuthorName);
            }
        }

        private string _Link;
        [DataMember(Name = "link")]
        [Column]
        public string Link
        {
            get { return _Link; }
            set
            {
                if (_Link == value) return;
                _Link = value;
                NotifyPropertyChanged(m => Link);
            }
        }

        private string _Title;
        [DataMember(Name = "title")]
        [Column]
        public string Title
        {
            get { return _Title; }
            set
            {
                if (_Title == value) return;
                _Title = value;
                NotifyPropertyChanged(m => Title);
            }
        }

        private string _Description;
        [DataMember(Name = "description")]
        [Column]
        public string Description
        {
            get { return _Description; }
            set
            {
                if (_Description == value) return;
                _Description = value;
                NotifyPropertyChanged(m => Description);
            }
        }

        private int? _AccentColor;
        [DataMember(Name = "accent_color")]
        [Column]
        public int? AccentColor
        {
            get { return _AccentColor; }
            set
            {
                if (_AccentColor == value) return;
                _AccentColor = value;
                NotifyPropertyChanged(m => AccentColor);
            }
        }

        private int? _Status;
        [DataMember(Name = "status")]
        [Column]
        public int? Status
        {
            get { return _Status; }
            set
            {
                if (_Status == value) return;
                _Status = value;
                NotifyPropertyChanged(m => Status);
            }
        }

        private bool? _Subscription;
        [Column]
        public bool? Subscription
        {
            get { return _Subscription; }
            set
            {
                if (_Subscription == value) return;
                _Subscription = value;
                NotifyPropertyChanged(m => Subscription);
                NotifyPropertyChanged(m => Notification);
            }
        }

        private bool? _Notification;
        [Column]
        public bool? Notification
        {
            get { return _Notification == true && _Subscription == true; }
            set
            {
                if (_Notification == value) return;
                _Notification = value;
                NotifyPropertyChanged(m => Notification);
            }
        }
    }
}
