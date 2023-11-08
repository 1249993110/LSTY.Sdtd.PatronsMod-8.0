using Epic.OnlineServices.Presence;
using IceCoffee.Common.Extensions;
using LSTY.Sdtd.PatronsMod.Extensions;
using Newtonsoft.Json.Linq;
using Noemax.GZip;
using System;
using System.Collections;
using System.IO;
using System.Net.Security;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace LSTY.Sdtd.PatronsMod.Commands
{
    public class RemoveItem : ConsoleCmdBase
    {
        protected override string[] getCommands()
        {
            return new string[]
            {
                "ty-RemoveItem",
                "ty-ri",
            };
        }

        protected override string getDescription()
        {
            return "";
        }
        
        private static byte[] AttachTags(string xmlName, byte[] compressedXmlData, List<string> addedTags)
        {
            using var memoryStream = new MemoryStream(compressedXmlData);
            using var deflateInputStream = new DeflateInputStream(memoryStream);
            using var decompressedMemoryStream = new MemoryStream();

            StreamUtils.StreamCopy(deflateInputStream, decompressedMemoryStream, null, true);

            var xmlDocument = new XmlDocument();
            decompressedMemoryStream.Seek(0, SeekOrigin.Begin);
            xmlDocument.Load(decompressedMemoryStream);

            var xmlNodeList = xmlDocument.SelectSingleNode(xmlName).ChildNodes;
            foreach (XmlNode item in xmlNodeList)
            {
                var xmlElement = (XmlElement)item;
                string itemName = xmlElement.GetAttribute("name");
                string tag = "ty_" + itemName;

                var currentNode = item.SelectSingleNode("property[@name='Tags']") as XmlElement;
                if (currentNode == null)
                {
                    var newElement = xmlDocument.CreateElement("property");
                    newElement.SetAttribute("name", "Tags");
                    newElement.SetAttribute("value", tag);
                    item.AppendChild(newElement);
                }
                else
                {
                    string[] tags = currentNode.GetAttribute("value").Split(',');
                    if (tags.Contains(tag) == false)
                    {
                        var _tags = tags.ToList();
                        _tags.Add(tag);
                        string newTags = string.Join(",", _tags);
                        currentNode.SetAttribute("value", newTags);
                    }
                }

                addedTags.Add(tag);
            }

            // using var newStream = new MemoryStream(decompressedMemoryStream.Capacity);
            // xmlDocument.Save(newStream);
            // SdFile.WriteAllBytes("C:/Users/Administrator/Desktop/temp1/" + xmlName + ".xml", newStream.ToArray());

            using var compressedMemoryStream = new MemoryStream(decompressedMemoryStream.Capacity);
            using var deflateOutputStream = new DeflateOutputStream(compressedMemoryStream, 3);
            xmlDocument.Save(deflateOutputStream);
            return compressedMemoryStream.ToArray();
        }

        private static byte[] GenerateGameevents(string xmlName, byte[] compressedXmlData, List<string> addedTags)
        {
            using var memoryStream = new MemoryStream(compressedXmlData);
            using var deflateInputStream = new DeflateInputStream(memoryStream);
            using var decompressedMemoryStream = new MemoryStream();

            StreamUtils.StreamCopy(deflateInputStream, decompressedMemoryStream, null, true);

            var xmlDocument = new XmlDocument();
            decompressedMemoryStream.Seek(0, SeekOrigin.Begin);
            xmlDocument.Load(decompressedMemoryStream);

            var rootNode = xmlDocument.SelectSingleNode(xmlName);

            foreach (var tag in addedTags)
            {
                var actionSequenceEl = xmlDocument.CreateElement("action_sequence");
                actionSequenceEl.SetAttribute("name", "action_" + tag);

                {
                    var actionEl = xmlDocument.CreateElement("action");
                    actionEl.SetAttribute("class", "RemoveItems");

                    var propertyEl = xmlDocument.CreateElement("property");
                    propertyEl.SetAttribute("name", "items_location");
                    propertyEl.SetAttribute("value", "Toolbelt,Equipment,Backpack");
                    actionEl.AppendChild(propertyEl);

                    propertyEl = xmlDocument.CreateElement("property");
                    propertyEl.SetAttribute("name", "items_tags");
                    propertyEl.SetAttribute("value", tag);
                    actionEl.AppendChild(propertyEl);

                    actionSequenceEl.AppendChild(actionEl);
                }

                {
                    var actionEl = xmlDocument.CreateElement("action");
                    actionEl.SetAttribute("class", "PlaySound");

                    var propertyEl = xmlDocument.CreateElement("property");
                    propertyEl.SetAttribute("name", "sound");
                    propertyEl.SetAttribute("value", "ui_trader_purchase");
                    actionEl.AppendChild(propertyEl);

                    propertyEl = xmlDocument.CreateElement("property");
                    propertyEl.SetAttribute("name", "inside_head");
                    propertyEl.SetAttribute("value", "true");
                    actionEl.AppendChild(propertyEl);

                    actionSequenceEl.AppendChild(actionEl);
                }

                rootNode.AppendChild(actionSequenceEl);
            }
            
            using var newStream = new MemoryStream(decompressedMemoryStream.Capacity);
            xmlDocument.Save(newStream);
            SdFile.WriteAllBytes("C:/Users/Administrator/Desktop/temp1/" + xmlName + ".xml", newStream.ToArray());

            using var compressedMemoryStream = new MemoryStream(decompressedMemoryStream.Capacity);
            using var deflateOutputStream = new DeflateOutputStream(compressedMemoryStream, 3);
            xmlDocument.Save(deflateOutputStream);
            return compressedMemoryStream.ToArray();
        }

        public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            // WorldStaticData.SaveXmlsToFolder("C:/Users/Administrator/Desktop/temp");

            if (typeof(WorldStaticData).GetField("xmlsToLoad", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) is IEnumerable xmlsToLoad)
            {
                var addedTags = new List<string>();

                Type? type = null;
                FieldInfo? nameField = null;
                FieldInfo? dataField = null;
                foreach (var item in xmlsToLoad)
                {
                    if (type == null || dataField == null || nameField == null)
                    {
                        type = item.GetType();
                        nameField = type.GetField("XmlName", BindingFlags.Instance | BindingFlags.Public);
                        dataField = type.GetField("CompressedXmlData", BindingFlags.Instance | BindingFlags.Public);
                    }

                    var xmlName = (string)nameField.GetValue(item);
                    if (xmlName == "items")
                    {
                        if (dataField.GetValue(item) is byte[] compressedXmlData)
                        {
                            var modified = AttachTags(xmlName, compressedXmlData, addedTags);
                            dataField.SetValue(item, modified);
                        }
                    }
                    else if (xmlName == "blocks")
                    {
                        if (dataField.GetValue(item) is byte[] compressedXmlData)
                        {
                            var modified = AttachTags(xmlName, compressedXmlData, addedTags);
                            dataField.SetValue(item, modified);
                        }
                    }
                    else if (xmlName == "gameevents")
                    {
                        if (dataField.GetValue(item) is byte[] compressedXmlData)
                        {
                            var modified = GenerateGameevents(xmlName, compressedXmlData, addedTags);
                            dataField.SetValue(item, modified);
                        }
                    }
                }
            }
        }
    }
}