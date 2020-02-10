//
// DevIntersectionLoader.cs
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
    public class DevIntersectionLoader : DataLoader
    {
        public override async Task LoadDataAsync(Stream fileStream, ApplicationDbContext db)
        {
            var reader = new JsonTextReader(new StreamReader(fileStream));

            var speakerNames = new Dictionary<string, Speaker>();
            var tracks = new Dictionary<string, Track>();

            JArray doc = await JArray.LoadAsync(reader);

            foreach (JObject item in doc)
            {
                var theseSpeakers = new List<Speaker>();
                foreach (var thisSpeakerName in item["speakerNames"])
                {
                    if (!speakerNames.ContainsKey(thisSpeakerName.Value<string>()))
                    {
                        var thisSpeaker = new Speaker { Name = thisSpeakerName.Value<string>() };
                        db.Speakers.Add(thisSpeaker);
                        speakerNames.Add(thisSpeakerName.Value<string>(), thisSpeaker);
                        Console.WriteLine(thisSpeakerName.Value<string>());
                    }
                    theseSpeakers.Add(speakerNames[thisSpeakerName.Value<string>()]);
                }

                var theseTracks = new List<Track>();
                foreach (var thisTrackName in item["trackNames"])
                {
                    if (!tracks.ContainsKey(thisTrackName.Value<string>()))
                    {
                        var thisTrack = new Track { Name = thisTrackName.Value<string>() };
                        db.Tracks.Add(thisTrack);
                        tracks.Add(thisTrackName.Value<string>(), thisTrack);
                    }
                    theseTracks.Add(tracks[thisTrackName.Value<string>()]);
                }

                var session = new Session
                {
                    Title = item["title"].Value<string>(),
                    StartTime = item["startTime"].Value<DateTime>(),
                    EndTime = item["endTime"].Value<DateTime>(),
                    Track = theseTracks[0],
                    Abstract = item["abstract"].Value<string>()
                };

                session.SessionSpeakers = new List<SessionSpeaker>();
                foreach (var sp in theseSpeakers)
                {
                    session.SessionSpeakers.Add(new SessionSpeaker
                    {
                        Session = session,
                        Speaker = sp
                    });
                }

                db.Sessions.Add(session);
            }
        }
    }
}
