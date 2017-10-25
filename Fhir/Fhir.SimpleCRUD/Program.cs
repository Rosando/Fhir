using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Fhir.SimpleCRUD
{
    public class Program
    {
        public static string FhirClientEndPoint = "https://fhirtest.uhn.ca/baseDstu3/";

        public static void Main(string[] args)
        {
            Patient createdPatient = CreatePatient("John", "Doe", new DateTime(1992, 12, 19));

            Patient readPatient = ReadPatient(createdPatient.Id);
        }

        public static Patient CreatePatient(string given, string family, DateTime birthDate)
        {
            Patient patient = new Patient();
            Patient responsePatient = new Patient();

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
                LogToFile("Creating Patient");

                LogToFile("Request: ");
                var patientXml = FhirSerializer.SerializeResourceToXml(patient);
                LogToFile(XDocument.Parse(patientXml).ToString());

                FhirClient fhirClient = new FhirClient(FhirClientEndPoint);
                responsePatient = fhirClient.Create(patient);

                LogToFile("Response: ");
                var responsePatientXml = FhirSerializer.SerializeResourceToXml(responsePatient);
                LogToFile(XDocument.Parse(responsePatientXml).ToString());
            }
            catch(Exception ex)
            {
                LogToFile(ex.ToString());
            }

            return responsePatient;
        }

        public static Patient ReadPatient(string patientId)
        {
            Patient responsePatient = new Patient();

            try
            {
                LogToFile("Read Patient");

                string location = $"Patient/{patientId}";
                LogToFile("Request: \n\n" + location);
                
                FhirClient fhirClient = new FhirClient(FhirClientEndPoint);
                responsePatient = fhirClient.Read<Patient>(location);

                LogToFile("Response: ");
                var responsePatientXml = FhirSerializer.SerializeResourceToXml(responsePatient);
                LogToFile(XDocument.Parse(responsePatientXml).ToString());
            }
            catch (Exception ex)
            {
                LogToFile(ex.ToString());
            }

            return responsePatient;
        }
        
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
