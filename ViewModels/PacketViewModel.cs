using BlazorApp1.Data;
using BlazorApp1.Helpers;
using BlazorApp1.Services;
using DynamicData;
using System.Data;
using ReactiveUI;

namespace BlazorApp1.ViewModels
{

    public class PacketViewModel : ReactiveObject
    {
        private PacketService PacketService_service;
        private SharedDataService SharedDataService_service;
        private ProjectViewModel ProjectVM;
        public PacketViewModel(SharedDataService sharedDataService, PacketService packetService, ProjectViewModel projectVM)
        {
            this.SharedDataService_service = sharedDataService;
            this.PacketService_service = packetService;
            ProjectVM = projectVM;
        }


        public async Task AddPacket(Packet newPacket)
        {
            await PacketService_service.AddPacket(newPacket);

            SharedDataService_service.MainProject = await ProjectVM.GetProject();
            
        }

        public async Task<Packet> GetPacket(Packet packet)
        {

            Packet? newPacket = await PacketService_service.GetSelectedPacket(packet);
            newPacket ??= new Packet()
                {
                    NotePackets = new List<NotePacket>(),

                };;

            SharedDataService_service.SelectedCard = newPacket;

            SharedDataService_service.SelectedNoteCard = SharedDataService_service.SelectedCard?.NotePackets.Count == 0 ? null : SharedDataService_service.SelectedNoteCard;

            List<Packet>? childPackets = await PacketService_service.GetChildPackets(packet); 
            if(childPackets is not null)
            {
                SharedDataService_service.ChildCards = childPackets;
            }
            
               // NotifyStateChanged();
                return newPacket;


        }

        public async Task<List<Packet>> GetPackets(string filterText)
        {
            return await PacketService_service.GetPackets(filterText);
        }

        public async Task<List<Packet>> GetPackets(bool Pinned)
        {
            return await PacketService_service.GetPackets(Pinned);
        }


        public async Task<List<Packet>> GetSelectionPackets(Packet packet)
        {
            return await PacketService_service.GetSelectionPackets(packet);
        }

        public async Task UpdatePacket(Packet packet)
        {

            await PacketService_service.UpdatePacket(packet);

        }

        public async Task DeletePacket(Packet packet)
        {
            if (!SharedDataService_service.filterPackets)
            {
               
                SharedDataService_service.MainProject.Packets.Remove(packet);
            }
            else
            {
                SharedDataService_service.FiltredPackets.Remove(packet);
            }
               
            await PacketService_service.DeletePacket(packet.PacketID);
            
            SharedDataService_service.showDeletePacketConfirmation = false;
            SharedDataService_service.ContextMenuCard = null;
        }


        public async Task SetParentPacket(Packet parentPacket, Packet childPacket)
        {
            childPacket.ParentID = parentPacket.PacketID;
            childPacket.Parent = null;
            childPacket.Selected = false;
            await UpdatePacket(childPacket);

           SharedDataService_service.MainProject = await ProjectVM.GetProject();
           SharedDataService_service.ContextMenuCard = null;

        }


        private List<Packet> _notesToPackeList = new List<Packet>();
        public List<Packet> NotesToPackeList
        {
            get => _notesToPackeList;
            set => this.RaiseAndSetIfChanged(ref _notesToPackeList, value);
        }

    }
}
