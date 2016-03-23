using PoeHUD.Framework;
using PoeHUD.Models;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.RemoteMemoryObjects;
using System.Collections.Generic;
using System.Linq;

namespace PoeHUD.Controllers
{
    public class GameController
    {
        public GameController(Memory memory)
        {
            Memory = memory;
            Area = new AreaController(this);
            EntityListWrapper = new EntityListWrapper(this);
            Window = new GameWindow(memory.Process);
            Game = new TheGame(memory);
            Files = new FsController(memory);
        }

        public EntityListWrapper EntityListWrapper { get; }
        public GameWindow Window { get; }
        public TheGame Game { get; }
        public AreaController Area { get; }

        public Memory Memory { get; }

        public IEnumerable<EntityWrapper> Entities => EntityListWrapper.Entities;

        public EntityWrapper Player => EntityListWrapper.Player;

        public bool InGame => Game.IngameState.InGame;

        public FsController Files { get; }

        public void RefreshState()
        {
            if (InGame)
            {
                EntityListWrapper.RefreshState();
                Area.RefreshState();
            }
        }

        public List<EntityWrapper> GetAllPlayerMinions() => Entities.Where(x => x.HasComponent<Player>()).SelectMany(c => c.Minions).ToList();

    }
}