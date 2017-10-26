using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Fhir.SimpleBundle
{
    public class Program
    {
        public static string FhirClientEndPoint = "https://fhirtest.uhn.ca/baseDstu3/";

        public static void Main(string[] args)
        {
            CreateResourcesIndividuallyAndFetchBundle();

            CreateResourcesFromBundle();
        }

        public static void CreateResourcesIndividuallyAndFetchBundle()
        {
            LogToFile("Begin CreateResourcesIndividuallyAndFetchBundle - " + DateTime.Now.ToString());

            Patient patient = CreatePatient("John", "Doe", new DateTime(1992, 12, 19));

            Patient responsePatient = SavePatient(patient);

            Condition condition = CreateCondition(responsePatient, "Case A", "High");

            Condition responseCondition = SaveCondition(condition);

            GetEverythingForPatient(responsePatient.Id);

            LogToFile("End CreateResourcesIndividuallyAndFetchBundle - " + DateTime.Now.ToString());
        }

        public static void CreateResourcesFromBundle()
        {
            LogToFile("Begin CreateResourcesFromBundle - " + DateTime.Now.ToString());

            Patient patient = CreatePatient("Jane", "Doe", new DateTime(1993, 07, 15));

            Condition condition = CreateCondition(patient, "Case 5", "Critical");

            Bundle bundle = CreateSaveBundle(patient, condition);

            string patientId = GetIndividualResourceFromBundle(bundle);

            GetEverythingForPatient(patientId);

            LogToFile("End CreateResourcesFromBundle - " + DateTime.Now.ToString());
        }

        public static Patient CreatePatient(string given, string family, DateTime birthDate)
        {
            Patient patient = new Patient();

            //Set patient Id
            patient.Id = Guid.NewGuid().ToString();

            //Set patient name
            patient.Name = new List<HumanName>();

            HumanName patientName = new HumanName();

            patientName.Use = HumanName.NameUse.Official;
            patientName.Prefix = new string[] { "Mr" };

            //Default way using properties to set Given and Family name
            //patientName.Given = new string[] { given };
            //patientName.Family = family;

            //Using methods to sets the Given and Family name
            patientName.WithGiven(given).AndFamily(family);

            patient.Name.Add(patientName);

            //Set patient Identifier
            patient.Identifier = new List<Identifier>();

            Identifier patientIdentifier = new Identifier();

            patientIdentifier.System = "http://someurl.net/not-sure-about-the-value/1.0";
            patientIdentifier.Value = Guid.NewGuid().ToString();

            patient.Identifier.Add(patientIdentifier);

            //Set patient birth date
            patient.BirthDate = birthDate.ToFhirDate();

            //Set Active Flag
            patient.Active = true;

            try
            {
                LogToFile("Created Patient Using POCO: ");
                
                var patientXml = FhirSerializer.SerializeResourceToXml(patient);
                LogToFile(XDocument.Parse(patientXml).ToString());
            }
            catch (Exception ex)
            {
                LogToFile(ex.ToString());
            }

            return patient;
        }

        public static Patient SavePatient(Patient patient)
        {
            Patient responsePatient = new Patient();

            try
            {
                FhirClient fhirClient = new FhirClient(FhirClientEndPoint);
                responsePatient = fhirClient.Create(patient);

                LogToFile("Patient Response After Creation: ");
                var responsePatientXml = FhirSerializer.SerializeResourceToXml(responsePatient);
                LogToFile(XDocument.Parse(responsePatientXml).ToString());
            }
            catch (Exception ex)
            {
                LogToFile(ex.ToString());
            }

            return responsePatient;
        }

        public static Condition CreateCondition(Patient patient, string category, string severity)
        {
            Condition condition = new Condition();

            condition.AssertedDate = DateTime.Now.ToFhirDate();
            condition.Category = new List<CodeableConcept>() { new CodeableConcept() { Text = category } };
            condition.Severity = new CodeableConcept() { Text = severity };

            ResourceReference resourceReference = new ResourceReference();
            resourceReference.ReferenceElement = new FhirString(patient.GetType().Name + "/" + patient.Id);

            condition.Subject = resourceReference;

            try
            {
                LogToFile("Created Condition Using POCO: ");

                var conditionXml = FhirSerializer.SerializeResourceToXml(condition);
                LogToFile(XDocument.Parse(conditionXml).ToString());
            }
            catch (Exception ex)
            {
                LogToFile(ex.ToString());

            }

            return condition;
        }

        public static Condition SaveCondition(Condition condition)
        {
            Condition responseCondition = new Condition();

            try
            {
                FhirClient fhirClient = new FhirClient(FhirClientEndPoint);
                responseCondition = fhirClient.Create(condition);

                LogToFile("Created Response After Creation: ");
                var responseConditionXml = FhirSerializer.SerializeResourceToXml(responseCondition);
                LogToFile(XDocument.Parse(responseConditionXml).ToString());
            }
            catch (Exception ex)
            {
                LogToFile(ex.ToString());
            }

            return responseCondition;
        }

        public static void GetEverythingForPatient(string patientId)
        {
            UriBuilder uriBuilder = new UriBuilder(FhirClientEndPoint);
            uriBuilder.Path = "Patient/" + patientId;

            try
            {
                FhirClient fhirClient = new FhirClient(FhirClientEndPoint);
                Resource resultResource = fhirClient.InstanceOperation(uriBuilder.Uri, "everything");

                var xml = FhirSerializer.SerializeResourceToXml(resultResource);
                LogToFile(XDocument.Parse(xml).ToString());
            }
            catch (Exception ex)
            {
                LogToFile(ex.ToString());
            }
        }

        public static Bundle CreateSaveBundle(Patient patient, Condition condition)
        {
            Bundle bundle = new Bundle();
            Bundle responseBundle = new Bundle();

            bundle.Type = Bundle.BundleType.Transaction;
            bundle.Entry = new List<Bundle.EntryComponent>()
            {
                new Bundle.EntryComponent()
                {
                    Resource = patient,
                    Request = new Bundle.RequestComponent() { Method = Bundle.HTTPVerb.POST }
                },
                new Bundle.EntryComponent()
                {
                    Resource = condition,
                    Request = new Bundle.RequestComponent() { Method = Bundle.HTTPVerb.POST }
                }
            };
            
            try
            {
                LogToFile("Created bundle with patient and condition using POCO: ");

                var xml = FhirSerializer.SerializeResourceToXml(bundle);
                LogToFile(XDocument.Parse(xml).ToString());

                FhirClient fhirClient = new FhirClient(FhirClientEndPoint);
                responseBundle = fhirClient.Transaction(bundle);

                LogToFile("Response bundle for patient and condition: ");

                var resultXml = FhirSerializer.SerializeResourceToXml(responseBundle);
                LogToFile(XDocument.Parse(resultXml).ToString());
            }
            catch (Exception ex)
            {
                LogToFile(ex.ToString());
            }

            return responseBundle;
        }

        public static string GetIndividualResourceFromBundle(Bundle bundle)
        {
            LogToFile("Get Individual Resource From Bundle:");

            string patientId = string.Empty;
            foreach (var entryItem in bundle.Entry)
            {
                Uri uri = new Uri(FhirClientEndPoint + entryItem.Response.Location);

                FhirClient fhirClient = new FhirClient(FhirClientEndPoint);
                var resource = fhirClient.Get(uri);

                var resourceXml = FhirSerializer.SerializeResourceToXml(resource);
                LogToFile(XDocument.Parse(resourceXml).ToString());

                if (resource.ResourceType == ResourceType.Patient)
                {
                    patientId = resource.Id;
                }
            }

            return patientId;
        }

        /// <summary>
        /// File location: bin\Debug\log.txt
        /// </summary>
        /// <param name="xml"></param>
        public static void LogToFile(string xml)
        {
            string path = Directory.GetCurrentDirectory() + "\\log.txt";

            if (!File.Exists(path))
            {
                var file = File.Create(path);
                file.Dispose();
            }

            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.Write(xml);
                sw.Write("\n\n");
            }
        }
    }
}
