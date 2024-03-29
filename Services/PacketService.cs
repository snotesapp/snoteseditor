﻿using BlazorApp1.Data;
using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;

namespace BlazorApp1.Services
{
    public class PacketService
    {
        private readonly ISqliteWasmDbContextFactory<SNotesDBContext> _dbContextFactory;
        public PacketService(ISqliteWasmDbContextFactory<SNotesDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            //using var db = _dbContextFactory.CreateDbContext();
            //db.Database.EnsureCreatedAsync();
        }



       
        public async Task<List<Packet>> GetPackets(bool Pinned)
        {
            using (var packetContext = await _dbContextFactory.CreateDbContextAsync())
            {
                if (Pinned)
                {
                    return await packetContext.Packets.Where(s => s.Selected == true).ToListAsync();
                }
                else
                {
                    return await packetContext.Packets.ToListAsync();
                }
               

            }

        }
        public async Task<List<Packet>> GetPackets(int? ParentID)
        {
            using (var packetContext = await _dbContextFactory.CreateDbContextAsync())
            {
                
                    return await packetContext.Packets.Where(p => p.ParentID == ParentID).ToListAsync();
               


            }

        }

        public async Task<List<Packet>> GetPackets(string filterText)
        {
            using (var packetContext = await _dbContextFactory.CreateDbContextAsync())
            {
                
                return await packetContext.Packets.Where(sl => sl.Title.ToLower().Contains(filterText.Trim().ToLower())).Include(pr => pr.Parent).ToListAsync();
            }
        }


        public async Task<Packet> GetSelectedPacket(Packet packet)
        {
            
            using (var packetContext = await _dbContextFactory.CreateDbContextAsync())
            {

                Packet? SelectedPacket = await packetContext.Packets.Where(c => c.PacketID == packet.PacketID)
                    .Include(nd => nd.NotePackets.OrderBy(od => od.Order))
                    .ThenInclude(nt => nt.Note)
                    .ThenInclude(im => im.Images)
                    .Include(nd => nd.NotePackets.OrderBy(od => od.Order))
                    .ThenInclude(nt => nt.Note)
                    .ThenInclude(pth => pth.NotePaths)
                    .Include(p => p.Parent)
                    .FirstOrDefaultAsync();


                return SelectedPacket;

            }
        }

        public async Task<List<Packet>> GetChildPackets(Packet packet)
        {
            try
            {
                using (var packetsContext = await _dbContextFactory.CreateDbContextAsync())
                {
                    return await packetsContext.Packets.Where(c => c.ParentID == packet.PacketID).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as desired
                Console.WriteLine("No SubPacket Found");
            }
            return new List<Packet>(); // return statement to return an empty list in case of an exception
        }

        public async Task<List<Packet>> GetSelectionPackets(Packet packet)
        {
            using (var packetsContext = await _dbContextFactory.CreateDbContextAsync())
            {

                List<Packet> chpackets = new List<Packet> { packet };
                int i = 0;
                do
                {
                    var templist = await GetChildPackets(chpackets[i]);
                    if (templist.Count > 0)
                    {
                        chpackets.AddRange(templist);
                    }

                    i++;
                } while (i < chpackets.Count);



                Task<List<Packet>> selpacket = packetsContext.Packets.Where(p => !chpackets.Select(p2 => p2.PacketID).Contains(p.PacketID)).OrderBy(p => p.Title).ToListAsync();
                return await selpacket;
            }
        }


        public async Task AddPacket(Packet packet)
        {
            using (var packetContext = await _dbContextFactory.CreateDbContextAsync())
            {

                await packetContext.Packets.AddAsync(packet);
                await packetContext.SaveChangesAsync();

            }

        }

        public async Task UpdatePacket(Packet packet)
        {
            using (var packetContext = await _dbContextFactory.CreateDbContextAsync())
            {

                packetContext.Packets.Update(packet);
                await packetContext.SaveChangesAsync();
            }

        }
       

        public async Task DeletePacket(int id)
        {
            using (var packetContext = await _dbContextFactory.CreateDbContextAsync())
            {

                var Packet = await packetContext.Packets.FindAsync(id);
                if (Packet != null)
                {
                    packetContext.Packets.Remove(Packet);
                    await packetContext.SaveChangesAsync();
                }

            }


        }


    }
}
