using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Runtime.Serialization;
using SimpleMvvmToolkit;
using System.ComponentModel;
using System;

namespace ZchMatome.Data
{
    [DataContract]
    [Table]
    public class FeedGroup : ModelBase<FeedGroup>
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

        private int? _Class;
        [DataMember(Name = "class")]
        [Column]
        public int? Class
        {
            get { return _Class; }
            set
            {
                if (_Class == value) return;
                _Class = value;
                NotifyPropertyChanged(m => Class);
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
            }
        }
    }
}
