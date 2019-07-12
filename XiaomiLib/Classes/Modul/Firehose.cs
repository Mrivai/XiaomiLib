

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage Firehost library
    /// </summary>
    public class Firehose
    {
        private static string _reset_to_edl = "<?xml version=\"1.0\" ?><data><power value=\"reset_to_edl\"/></data>";
        private static string _set_boot_partition = "<?xml version=\"1.0\" ?><data><setbootablestoragedrive value=\"1\"/></data>";
        private static string _nop = "<?xml version=\"1.0\" ?><data><nop value=\"ping\"/></data>";
        private static string _configure = "<?xml version=\"1.0\" ?><data><configure ZlpAwareHost=\"1\" MaxPayloadSizeToTargetInBytes=\"{0}\" MemoryName=\"{1}\" SkipStorageInit=\"{2}\"/></data>";

        /// <summary>
        /// Reset to EDL
        /// </summary>
        public static string Reset_To_Edl
        {
            get
            {
            return _reset_to_edl;
            }
        }
        /// <summary>
        /// Firehost set boot partition
        /// </summary>
        public static string SetBootPartition
        {
          get
          {
            return _set_boot_partition;
          }
        }
        /// <summary>
        /// Firehost nop  
        /// </summary>
        public static string Nop
        {
          get
          {
            return Firehose._nop;
          }
        }
        /// <summary>
        /// Firehost configure
        /// </summary>
        public static string Configure
        {
          get
          {
            return _configure;
          }
        }
        /// <summary>
        /// Firehost payload
        /// </summary>
        public static int payload_size
        {
          get
          {
            return 1048576;
          }
        }
        /// <summary>
        /// Firehost max pact len
        /// </summary>
        public static int MAX_PATCH_VALUE_LEN
        {
          get
          {
            return 50;
          }
        }
        /// <summary>
        /// Firehost Program
        /// </summary>
        public static string FIREHOSE_PROGRAM
        {
          get
          {
            return "<?xml version=\"1.0\" ?><data><program SECTOR_SIZE_IN_BYTES=\"{0}\" num_partition_sectors=\"{1}\" start_sector=\"{2}\" physical_partition_number=\"{3}\"/></data>";
          }
        }
        /// <summary>
        /// Firehost patch
        /// </summary>
        public static string FIREHOSE_PATCH
        {
          get
          {
            return "<?xml version=\"1.0\" ?><data><patch SECTOR_SIZE_IN_BYTES=\"{0}\" byte_offset=\"{1}\" filename=\"DISK\" physical_partition_number=\"{2}\" size_in_bytes=\"{3}\" start_sector=\"{4}\" value=\"{5}\" what=\"Update\"/></data>";
          }
        }
      }
}
