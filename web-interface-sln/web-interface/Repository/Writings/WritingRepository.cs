﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using WebInterface.Model;

namespace WebInterface.Repository.Writings
{
    public class WritingRepository : IWritingRepository
    {
        private DirectoryInfo directory;

        public WritingRepository(string directory)
        {
            this.directory = Directory.CreateDirectory(directory);
        }

        /// <summary>
        /// Gets data about a writing
        /// </summary>
        /// <param name="writerId">writerId</param>
        /// <param name="writingId">writerId-writingDir_filenameWithoutExtension</param>
        /// <returns>The writing</returns>
        Writing IWritingRepository.Get(string writerId, string writingId)
        {
            string writingDir = writingId.Substring(0, writingId.IndexOf('_'));
            string filename = writingId.Substring(writingId.IndexOf('_') + 1) + ".xml";
            var file = directory.EnumerateDirectories()
                .FirstOrDefault(x => x.Name == writerId)
                ?.EnumerateDirectories()
                .FirstOrDefault(x => x.Name == writingDir)
                ?.EnumerateFiles()
                .FirstOrDefault(x => x.Name == filename);
            if (file == null)
            {
                return null;
            }
            XDocument doc;
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
            {
                doc = XDocument.Load(fileStream);
            }
            // General info
            var generalElement = doc.Root.Element("General");
            // Capture time
            var captureTimeElement = generalElement.Element("CaptureTime");
            var captureTime = new DateTime(Int32.Parse(captureTimeElement.Attribute("year").Value),
                Int32.Parse(captureTimeElement.Attribute("month").Value),
                Int32.Parse(captureTimeElement.Attribute("dayOfMonth").Value));
            // Transcription
            var transcriptionElement = doc.Root.Element("Transcription");
            var textElement = transcriptionElement.Element("Text");
            string text;
            using (var reader = textElement.CreateReader())
            {
                reader.MoveToContent();
                text = reader.ReadInnerXml().Trim();
            }
            // Strokes
            var strokeSetElement = doc.Root.Element("StrokeSet");
            var strokes = strokeSetElement.Elements("Stroke")
                .Select(stroke => new Stroke()
                {
                    Points = stroke.Elements("Point")
                        .Select(point => new Point
                        {
                            X = Int32.Parse(point.Attribute("x").Value),
                            Y = Int32.Parse(point.Attribute("y").Value),
                            Time = point.Attribute("time").Value
                        })
                });
            return new Writing()
            {
                WriterId = writerId,
                WritingId = writingId,
                CaptureTime = captureTime,
                Text = text,
                Strokes = strokes
            };
        }

        public object SetLine(string writerId, string writingId, int strokeIndex, string type)
        {
            string writingDir = writingId.Substring(0, writingId.IndexOf('_'));
            string filename = writingId.Substring(writingId.IndexOf('_') + 1) + ".xml";
            var file = directory.EnumerateDirectories()
                .FirstOrDefault(x => x.Name == writerId)
                ?.EnumerateDirectories()
                .FirstOrDefault(x => x.Name == writingDir)
                ?.EnumerateFiles()
                .FirstOrDefault(x => x.Name == filename);
            if (file == null)
            {
                return null;
            }
            XDocument doc;
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
            {
                doc = XDocument.Load(fileStream);
            }
            // Strokes
            var strokeSetElement = doc.Root.Element("StrokeSet");
            var stroke = strokeSetElement.Elements("Stroke").ElementAt(strokeIndex);
            stroke.SetAttributeValue("isHorizontalStroke", true);
            stroke.SetAttributeValue("strokeDirection", type);
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
            {
                using (XmlWriter writer = XmlWriter.Create(fileStream, settings))
                {
                    doc.Save(writer);
                }
            }
            return "Successful";
        }

        public object RemoveLine(string writerId, string writingId, int strokeIndex)
        {
            string writingDir = writingId.Substring(0, writingId.IndexOf('_'));
            string filename = writingId.Substring(writingId.IndexOf('_') + 1) + ".xml";
            var file = directory.EnumerateDirectories()
                .FirstOrDefault(x => x.Name == writerId)
                ?.EnumerateDirectories()
                .FirstOrDefault(x => x.Name == writingDir)
                ?.EnumerateFiles()
                .FirstOrDefault(x => x.Name == filename);
            if (file == null)
            {
                return null;
            }
            XDocument doc;
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
            {
                doc = XDocument.Load(fileStream);
            }
            // Strokes
            var strokeSetElement = doc.Root.Element("StrokeSet");
            var stroke = strokeSetElement.Elements("Stroke").ElementAt(strokeIndex);
            stroke.SetAttributeValue("isHorizontalStroke", null);
            stroke.SetAttributeValue("strokeDirection", null);
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
            {
                using (XmlWriter writer = XmlWriter.Create(fileStream, settings))
                {
                    doc.Save(writer);
                }
            }
            return "Successful";
        }
    }
}
