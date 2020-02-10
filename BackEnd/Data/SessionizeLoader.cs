//
// SessionizeLoader.cs
//
// Author:
//       Sai Muppa <snmuppa@gmail.com>
//
// Copyright (c) 2020 (c) Sai Muppa
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackEnd.Data
{
    public class SessionizeLoader : DataLoader
    {
        public override async Task LoadDataAsync(Stream fileStream, ApplicationDbContext db)
        {
            var addedSpeakers = new Dictionary<string, Speaker>();
            var addedTracks = new Dictionary<string, Track>();

            var array = await JToken.LoadAsync(new JsonTextReader(new StreamReader(fileStream)));

            var root = array.ToObject<List<RootObject>>();

            foreach (var date in root)
            {
                foreach (var room in date.Rooms)
                {
                    if (!addedTracks.ContainsKey(room.Name))
                    {
                        var thisTrack = new Track { Name = room.Name };
                        db.Tracks.Add(thisTrack);
                        addedTracks.Add(thisTrack.Name, thisTrack);
                    }

                    foreach (var thisSession in room.Sessions)
                    {
                        foreach (var speaker in thisSession.Speakers)
                        {
                            if (!addedSpeakers.ContainsKey(speaker.Name))
                            {
                                var thisSpeaker = new Speaker { Name = speaker.Name };
                                db.Speakers.Add(thisSpeaker);
                                addedSpeakers.Add(thisSpeaker.Name, thisSpeaker);
                            }
                        }

                        var session = new Session
                        {
                            Title = thisSession.Title,
                            StartTime = thisSession.StartsAt,
                            EndTime = thisSession.EndsAt,
                            Track = addedTracks[room.Name],
                            Abstract = thisSession.Description
                        };

                        session.SessionSpeakers = new List<SessionSpeaker>();
                        foreach (var sp in thisSession.Speakers)
                        {
                            session.SessionSpeakers.Add(new SessionSpeaker
                            {
                                Session = session,
                                Speaker = addedSpeakers[sp.Name]
                            });
                        }

                        db.Sessions.Add(session);
                    }
                }
            }
        }

        private class RootObject
        {
            public DateTime Date { get; set; }
            public List<Room> Rooms { get; set; }
            public List<TimeSlot> TimeSlots { get; set; }
        }

        private class ImportSpeaker
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        private class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<object> CategoryItems { get; set; }
            public int Sort { get; set; }
        }

        private class ImportSession
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime StartsAt { get; set; }
            public DateTime EndsAt { get; set; }
            public bool IsServiceSession { get; set; }
            public bool IsPlenumSession { get; set; }
            public List<ImportSpeaker> Speakers { get; set; }
            public List<Category> Categories { get; set; }
            public int RoomId { get; set; }
            public string Room { get; set; }
        }

        private class Room
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<ImportSession> Sessions { get; set; }
            public bool HasOnlyPlenumSessions { get; set; }
        }

        private class TimeSlot
        {
            public string SlotStart { get; set; }
            public List<Room> Rooms { get; set; }
        }
    }
}
