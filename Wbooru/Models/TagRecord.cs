using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel;
using Wbooru.Kernel.ManagerImpl;
using Wbooru.Models;

namespace Wbooru.Models
{
    public class TagRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TagID { get; set; }

        public Tag Tag { get; set; }
        public DateTime AddTime { get; set; }
        public string FromGallery { get; set; }

        private TagRecordType recordType;
        [Column("RecordType")]
        public TagRecordType RecordType
        {
            get
            {
                return recordType;
            }
            set
            {
                OnRecordTypeChanged(recordType, value);
                recordType = value;
            }
        }

        private void OnRecordTypeChanged(TagRecordType oldValue, TagRecordType newValue)
        {
            bool checkFlag(TagRecordType f, out bool isContain)
            {
                var o = oldValue.HasFlag(f);
                var n = newValue.HasFlag(f);
                isContain = n;
                return o != n;
            }

            App.Current.Dispatcher.InvokeAsync(() =>
            {
                var tagManager = Container.Get<ITagManager>();
                if (checkFlag(TagRecordType.Filter, out var isContain))
                {
                    if (isContain)
                    {
                        tagManager.FiltedTags.Add(this);
                    }
                    else
                    {
                        tagManager.FiltedTags.Remove(this);
                    }
                }
                if (checkFlag(TagRecordType.Marked, out isContain))
                {
                    if (isContain)
                    {
                        tagManager.MarkedTags.Add(this);
                    }
                    else
                    {
                        tagManager.MarkedTags.Remove(this);
                    }
                }
                if (checkFlag(TagRecordType.Subscribed, out isContain))
                {
                    if (isContain)
                    {
                        tagManager.SubscribedTags.Add(this);
                    }
                    else
                    {
                        tagManager.SubscribedTags.Remove(this);
                    }
                }
            });
        }

        [Flags]
        public enum TagRecordType
        {
            None = 0,//for tag type cache
            Filter = 2 << 1,
            Marked = 2 << 2,
            Subscribed = Marked | 2 << 3
        }
    }
}
