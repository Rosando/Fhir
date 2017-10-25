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
            LogToFile("Begin CRUD - " + DateTime.Now.ToString());

            Patient patient = CreatePatient("John", "Doe", new DateTime(1992, 12, 19));

            Condition condition = CreateCondition(patient, "Case A", "High");

            //Bundle bundle = new Bundle();
            //bundle.Type = Bundle.BundleType.Transaction;
            //bundle.Entry = new List<Bundle.EntryComponent>()
            //{
            //    new Bundle.EntryComponent()
            //    {
            //        Resource = patient,
            //        Request = new Bundle.RequestComponent() { Method = Bundle.HTTPVerb.POST }
            //    },
            //    new Bundle.EntryComponent()
            //    {
            //        Resource = condition,
            //        Request = new Bundle.RequestComponent() { Method = Bundle.HTTPVerb.POST }
            //    }
            //};

            //Bundle resultBundle = new Bundle();

            //try
            //{
            //    var xml = FhirSerializer.SerializeResourceToXml(bundle);
            //    LogToFile(XDocument.Parse(xml).ToString());

            //    resultBundle = fhirClient.Transaction(bundle);
            //    //result.

            //    var resultXml = FhirSerializer.SerializeResourceToXml(resultBundle);
            //    LogToFile(XDocument.Parse(resultXml).ToString());
            //}
            //catch (Exception ex)
            //{
            //    LogToFile(ex.ToString());
            //}

            LogToFile("End CRUD - " + DateTime.Now.ToString());
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
                LogToFile("Patient: ");
                
                var patientXml = FhirSerializer.SerializeResourceToXml(patient);
                LogToFile(XDocument.Parse(patientXml).ToString());
            }
            catch (Exception ex)
            {
                LogToFile(ex.ToString());
            }

            return patient;
        }

        public static Condition CreateCondition(Patient patient, string category, string severity)
        {
            Condition condition = new Condition();

            condition.AssertedDate = DateTime.Now.ToFhirDate();
            condition.Category = new List<CodeableConcept>() { new CodeableConcept() { Text = category } };
            condition.Severity = new CodeableConcept() { Text = severity };

            ResourceReference resourceReference = new ResourceReference();
            resourceReference.ReferenceElement = new FhirString(patient.GetType().Name + "/" + patient.Id);

            try
            {
                LogToFile("Condition: ");

                var conditionXml = FhirSerializer.SerializeResourceToXml(condition);
                LogToFile(XDocument.Parse(conditionXml).ToString());
            }
            catch (Exception ex)
            {
                LogToFile(ex.ToString());

            }

            return condition;
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
