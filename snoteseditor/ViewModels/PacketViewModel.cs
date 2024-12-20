﻿using BlazorApp1.Data;
using BlazorApp1.Helpers;
using BlazorApp1.Services;
using DynamicData;
using System.Data;
using ReactiveUI;
using BlazorBootstrap;

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


        public Packet? PrevPacket { get; set; }
        private Packet? _nexPacket;
        public Packet? NextPacket
        {        
            get { return _nexPacket; }
            set { this.RaiseAndSetIfChanged(ref _nexPacket, value);  }
        }

        public bool editNotePacketNote = false;
        public bool NotesMenuSelected = false;

        public NotePacket NotePacketToDel ;

        #region NotePacketNavigatior
        public bool leftAnimation, rightAnimation, upAnimation, downAnimation = false;

        public bool NavUp , NavDown, NavNext, NavPrev;
        public bool UpToolTipTxt, NextToolTipTxt, PrevToolTipTxt;


        #endregion

        public ConfirmDialog deletePacketDialog = default!;
        public Modal movePacketTo_Modal = default!;
        public Modal movedownPacketTo_Modal = default!;
        public Modal addNoteToPacket_Modal = default!;

        public async Task AddPacket(Packet newPacket)
        {
            await PacketService_service.AddPacket(newPacket);
          
            SharedDataService_service.MainProject.Packets.Add(newPacket);
           
            //SharedDataService_service.MainProject = await ProjectVM.GetProject();
            NotifyStateChanged();
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
                SharedDataService_service.CurrentPacketsSet = childPackets;
            }


          
               // NotifyStateChanged();
                return newPacket;


        }

        public async Task GetPrevOrNextPackets(Packet? CurrentPacket,bool IsRootPackets)
        {

            if (CurrentPacket is not null)
            {
                List<Packet> NeighborPackets = await PacketService_service.GetPackets(ParentID: CurrentPacket.ParentID);
              
                    int targetIndex = NeighborPackets.IndexOf((NeighborPackets.FirstOrDefault(id => id.PacketID == CurrentPacket.PacketID)));
                    if (targetIndex != -1)
                    {
                        PrevPacket = NeighborPackets.ElementAtOrDefault(targetIndex - 1);
                        NextPacket = NeighborPackets.ElementAtOrDefault(targetIndex + 1);


                    }
                    else
                    {
                        Console.WriteLine("Element not found in the collection.");
                    }
                
                
            }

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
            
            SharedDataService_service.ContextMenuCard = null;
        }


        public async Task SetParentPacket(Packet parentPacket, Packet childPacket)
        {
            childPacket.ParentID = parentPacket.PacketID;
            childPacket.Parent = null;
            childPacket.Selected = false;
            await UpdatePacket(childPacket);

           SharedDataService_service.MainProject = await ProjectVM.GetProject();
           await movePacketTo_Modal.HideAsync();
           NotifyStateChanged();
           /*
           SharedDataService_service.ContextMenuCard = null;
           SharedDataService_service.moveto_dialog = false;
            */
        }


        public async Task DeleteNotePacket(NotePacket noteCard)
    {
        int indx = SharedDataService_service.SelectedCard.NotePackets.IndexOf(noteCard);

        if (indx == 0 && SharedDataService_service.SelectedCard.NotePackets.Count > 1)
        {


            SharedDataService_service.SelectedCard.NotePackets.Remove(noteCard);
            await SharedDataService_service.RemoveNoteCard(noteCard);
            SharedDataService_service.SelectedNoteCard = SharedDataService_service.SelectedCard.NotePackets.First();
        }
        else if (SharedDataService_service.SelectedCard.NotePackets.Count == 1)
        {
            SharedDataService_service.SelectedNoteCard = null;
            await SharedDataService_service.RemoveNoteCard(noteCard);

            await GetPacket(SharedDataService_service.SelectedCard);
            SharedDataService_service.SwitchMenus("cards");
        }
        else if (indx < SharedDataService_service.SelectedCard.NotePackets.Count)
        {
            SharedDataService_service.SelectedNoteCard = SharedDataService_service.SelectedCard.NotePackets.ElementAtOrDefault(indx - 1);
            SharedDataService_service.SelectedCard.NotePackets.Remove(noteCard);
            await SharedDataService_service.RemoveNoteCard(noteCard);
        }
        else
        {
            await GetPacket(SharedDataService_service.SelectedCard);
            SharedDataService_service.SwitchMenus("cards");
        }

    }


        private List<Packet> _notesToPackeList = new List<Packet>();
        public List<Packet> NotesToPackeList
        {
            get => _notesToPackeList;
            set => this.RaiseAndSetIfChanged(ref _notesToPackeList, value);
        }

        public event Action OnChange;
        public void NotifyStateChanged() => OnChange?.Invoke();

    }

    
}
