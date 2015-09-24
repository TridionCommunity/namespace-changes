using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Tridion.ContentManager;
using Tridion.ContentManager.CoreService.Client;
using ItemType = Tridion.ContentManager.CoreService.Client.ItemType;

namespace ChangeNamespaceOnComponents
{
    class Program
    {
        static void Main(string[] args)
        {
            //args[0] = "tcm:11-403-8";
            if (!args.Any())
            {
                Log("Please pass the Schema Tcm Uri as a parameter.");
                return;
            }
            string schemaUri = args[0];
            if (!TcmUri.IsValid(schemaUri))
            {
                Log("The specified URI of " + schemaUri + " is not a valid URI, please pass the schema Tcm Uri as a parameter.");
                return;
            }

            SessionAwareCoreServiceClient client = new SessionAwareCoreServiceClient("netTcp_2013");
            if (!client.IsExistingObject(schemaUri))
            {
                Log("Could not find item with URI " + schemaUri + " in Tridion. Please pass the Schema Tcm Uri as a parameter.");
                return;
            }
            ReadOptions readOptions = new ReadOptions();
            UsingItemsFilterData whereUsedFilter = new UsingItemsFilterData { ItemTypes = new[] { ItemType.Component } };
            SchemaData schema = (SchemaData)client.Read(schemaUri, readOptions);
            SchemaFieldsData schemaFieldsData = client.ReadSchemaFields(schema.Id, true, readOptions);
            bool hasMeta = schemaFieldsData.MetadataFields.Any();
            string newNamespace = schema.NamespaceUri;

            if (schema.Purpose == SchemaPurpose.Metadata)
            {
                List<IdentifiableObjectData> items = new List<IdentifiableObjectData>();
                UsingItemsFilterData anyItem = new UsingItemsFilterData();
                foreach (XElement node in client.GetListXml(schema.Id, anyItem).Nodes())
                {
                    string uri = node.Attribute("ID").Value;
                    items.Add(client.Read(uri, readOptions));
                }
                Log("Found " + items.Count + " items using schema...");

                foreach (var item in items)
                {
                    if (item is PublicationData)
                    {
                        PublicationData pub = (PublicationData)item;
                        string meta = pub.Metadata;
                        XmlDocument xml = new XmlDocument();
                        xml.LoadXml(meta);
                        string oldnamespace = xml.DocumentElement.NamespaceURI;
                        if (oldnamespace != newNamespace)
                        {
                            Log("Replacing namespace for publication " + pub.Id + " (" + pub.Title + ") - Current Namespace: " + oldnamespace);
                            string metadata = meta.Replace(oldnamespace, newNamespace);
                            pub.Metadata = metadata;
                            client.Update(pub, readOptions);
                        }
                    }
                    else if (item is RepositoryLocalObjectData)
                    {
                        RepositoryLocalObjectData data = (RepositoryLocalObjectData)item;
                        string meta = data.Metadata;
                        XmlDocument xml = new XmlDocument();
                        xml.LoadXml(meta);
                        string oldnamespace = xml.DocumentElement.NamespaceURI;
                        if (oldnamespace != newNamespace)
                        {
                            Log("Replacing namespace for item " + data.Id + " (" + data.Title + ") - Current Namespace: " + oldnamespace);
                            string metadata = meta.Replace(oldnamespace, newNamespace);
                            data.Metadata = metadata;
                            client.Update(data, readOptions);
                        }

                    }
                }

                return;
            }

            List<ComponentData> components = new List<ComponentData>();
            foreach (XElement node in client.GetListXml(schema.Id, whereUsedFilter).Nodes())
            {
                string uri = node.Attribute("ID").Value;
                components.Add((ComponentData)client.Read(uri, readOptions));
            }
            Log("Found " + components.Count + " components.");


            Log("Current schema namespace set to " + newNamespace + ", checking for components with incorrect namespace.");
            int count = 0;
            foreach (var component in components)
            {
                if (schema.Purpose == SchemaPurpose.Multimedia)
                {
                    Log("Changing Multimedia Component");
                    string meta = component.Metadata;
                    XmlDocument metaXml = new XmlDocument();
                    metaXml.LoadXml(meta);
                    string metaOldnamespace = metaXml.DocumentElement.NamespaceURI;
                    if (metaOldnamespace != newNamespace)
                    {
                        Log("Replacing namespace for item " + component.Id + " (" + component.Title + ") - Current Namespace: " + metaOldnamespace);
                        string metadata = meta.Replace(metaOldnamespace, newNamespace);
                        component.Metadata = metadata;
                        client.Update(component, readOptions);
                    }
                    count++;
                    Log(components.Count - count + " components remaining...");

                    continue;
                }

                string content = component.Content;

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(content);


                string oldnamespace = xml.DocumentElement.NamespaceURI;

                if (oldnamespace != newNamespace)
                {
                    Log("Replacing namespace for component " + component.Id + " (" + component.Title + ") - Current Namespace: " + oldnamespace);
                    content = content.Replace(oldnamespace, newNamespace);
                    try
                    {
                        ComponentData editableComponent = component;
                        editableComponent.Content = content;
                        if (hasMeta)
                        {
                            string metadata = editableComponent.Metadata.Replace(oldnamespace, newNamespace);

                            // Fix for new meta
                            if (string.IsNullOrEmpty(metadata))
                            {
                                metadata = string.Format("<Metadata xmlns=\"{0}\" />", newNamespace);
                                Log("Component had no metadata, but schema specifies it has. Adding empty metadata node");
                            }
                            editableComponent.Metadata = metadata;
                        }

                        if (!hasMeta && !(string.IsNullOrEmpty(editableComponent.Metadata)))
                        {
                            editableComponent.Metadata = string.Empty;
                        }

                        client.Update(editableComponent, readOptions);

                    }
                    catch (Exception ex)
                    {
                        Log("Error occurred trying to update component: " + component.Id + Environment.NewLine + ex);

                    }

                }
                count++;
                Log(components.Count - count + " components remaining...");
            }

        }

        static void Log(string message)
        {
            string logMessage = string.Format("[{0}] {1} {2}", DateTime.Now.ToLongTimeString(), message,
                                              Environment.NewLine);
            File.AppendAllText("ChangeNamespaceOnComponent.log", logMessage);
            Console.WriteLine(logMessage);

        }
    }
}
