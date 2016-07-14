using PoeHUD.Framework;
using PoeHUD.Poe.FilesInMemory;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PoeHUD.Controllers
{
    public class FsController
    {
        public readonly BaseItemTypes BaseItemTypes;
        public readonly ItemClassesDisplay ItemClassesDisplay;
        public readonly ModsDat Mods;
        public readonly StatsDat Stats;
        public readonly TagsDat Tags;
        private readonly Dictionary<string, int> files;
        private readonly Memory mem;
        private bool isLoaded;

        public FsController(Memory mem)
        {
            this.mem = mem;
            files = GetAllFiles();
            ItemClassesDisplay = new ItemClassesDisplay();
            BaseItemTypes = new BaseItemTypes(mem, FindFile("Data/BaseItemTypes.dat"), ItemClassesDisplay);
            Tags = new TagsDat(mem, FindFile("Data/Tags.dat"));
            Stats = new StatsDat(mem, FindFile("Data/Stats.dat"));
            Mods = new ModsDat(mem, FindFile("Data/Mods.dat"), Stats, Tags);
        }

        public Dictionary<string, int> GetAllFiles()
        {
            var fileList = new Dictionary<string, int>();
            int fileRoot = mem.AddressOfProcess + mem.offsets.FileRoot;
            int count = mem.ReadInt(fileRoot + 0x8);
            int start = mem.ReadInt(fileRoot + 0xC, 0x18);
            for (int i = 0; i < count; i++)
            {
                start = mem.ReadInt(start);
                var str = mem.ReadStringU(mem.ReadInt(start + 8), 512);
                if (!fileList.ContainsKey(str))
                    fileList.Add(str, start);
            }
            return fileList;
        }

        public int FindFile(string name)
        {
            try
            {
                return files[name];
            }
            catch (KeyNotFoundException)
            {
                const string MESSAGE_FORMAT = "Couldn't find the file in memory: {0}\nTry to restart the game.";
                MessageBox.Show(string.Format(MESSAGE_FORMAT, name), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            return 0;
        }
    }
}