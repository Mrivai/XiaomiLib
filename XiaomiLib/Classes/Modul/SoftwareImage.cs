

using System.Collections;
using System.Collections.Generic;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manages Regex Match
    /// </summary>
    public class SoftwareImage
    {
        /// <summary>
        /// regex match programmer file
        /// </summary>
        public static string ProgrammerPattern
        {
            get
            {
                return "MPRG.*.hex|MPRG.*.mbn|prog_.*_firehose_.*.*";
            }
        }
        /// <summary>
        /// regex match provision xml
        /// </summary>
        public static string ProvisionPattern
        {
            get
            {
                return "provision.*\\.xml";
            }
        }
        /// <summary>
        /// regex match raw program file
        /// </summary>
        public static string RawProgramPattern
        {
            get
            {
                return "rawprogram.*\\.xml";
            }
        }
        /// <summary>
        /// regex match patch patern
        /// </summary>
        public static string PatchPattern
        {
            get
            {
                return "patch.*\\.xml";
            }
        }

        
        /// <summary>
        /// regex match device firmware file
        /// </summary>
        public static Hashtable DummyProgress
        {
            get
            {
                return new Hashtable()
                {
                  {
                     "xbl.elf",
                     1
                  },
                  {
                     "tz.mbn",
                     2
                  },
                  {
                     "hyp.mbn",
                     3
                  },
                  {
                     "rpm.mbn",
                     4
                  },
                  {
                     "emmc_appsboot.mbn",
                     5
                  },
                  {
                     "pmic.elf",
                     6
                  },
                  {
                     "devcfg.mbn",
                     7
                  },
                  {
                     "BTFM.bin",
                     8
                  },
                  {
                     "cmnlib.mbn",
                     9
                  },
                  {
                     "cmnlib64.mbn",
                     10
                  },
                  {
                     "NON-HLOS.bin",
                     11
                  },
                  {
                     "adspso.bin",
                     12
                  },
                  {
                     "mdtp.img",
                     13
                  },
                  {
                     "keymaster.mbn",
                     14
                  },
                  {
                     "misc.img",
                     15
                  },
                  {
                     "system.img",
                     16
                  },
                  {
                     "cache.img",
                     30
                  },
                  {
                     "userdata.img",
                     34
                  },
                  {
                     "recovery.img",
                     35
                  },
                  {
                     "splash.img",
                     36
                  },
                  {
                     "logo.img",
                     37
                  },
                  {
                     "boot.img",
                     38
                  },
                  {
                     "cust.img",
                     45
                  }
                };
            }
        }
    }
}
