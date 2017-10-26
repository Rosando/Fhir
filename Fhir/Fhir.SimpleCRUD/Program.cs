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
            LogToFile("Begin CRUD - " + DateTime.Now.ToString());

            Patient createdPatient = CreatePatient("John", "Doe", new DateTime(1992, 12, 19));

            Patient readCreatedPatient = ReadPatient(createdPatient.Id);

            Patient updatePatient = UpdatePatientBirthDate(readCreatedPatient, new DateTime(1991, 12, 20));

            Patient readUpdatedPatient = ReadPatient(updatePatient.Id);

            //DeletePatient(readUpdatedPatient);
            //DeletePatientByEndPoint(readUpdatedPatient.Id);
            DeletePatientBySearchParams(readUpdatedPatient);

            Patient readDeletedPatient = ReadPatient(readUpdatedPatient.Id);

            LogToFile("End CRUD - " + DateTime.Now.ToString());
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

        public static Patient UpdatePatientBirthDate(Patient patient, DateTime birthDate)
        {
            Patient responsePatient = new Patient();

            patient.BirthDate = birthDate.ToFhirDate();

            try
            {
                LogToFile("Update Patient");

                LogToFile("Request: ");
                var patientXml = FhirSerializer.SerializeResourceToXml(patient);
                LogToFile(XDocument.Parse(patientXml).ToString());

                FhirClient fhirClient = new FhirClient(FhirClientEndPoint);
                responsePatient = fhirClient.Update(patient);

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
        
        public static void DeletePatient(Patient patient)
        {
            try
            {
                LogToFile("Delete Patient");

                LogToFile("Request: ");
                var patientXml = FhirSerializer.SerializeResourceToXml(patient);
                LogToFile(XDocument.Parse(patientXml).ToString());

                FhirClient fhirClient = new FhirClient(FhirClientEndPoint);
                fhirClient.Delete(patient);

                LogToFile("Response: Delete has no response");
            }
            catch (Exception ex)
            {
                LogToFile(ex.ToString());
            }
        }

        public static void DeletePatientByEndPoint(string patientId)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder(FhirClientEndPoint + "Patient/" + patientId);

                LogToFile("Delete Patient");

                LogToFile("Request: ");
                LogToFile(uriBuilder.Uri.ToString());

                FhirClient fhirClient = new FhirClient(FhirClientEndPoint);
                fhirClient.Delete(uriBuilder.Uri);

                LogToFile("Response: Delete has no response");
            }
            catch (Exception ex)
            {
                LogToFile(ex.ToString());
            }
        }

        /// <summary>
        /// Value search parameters for this search are: 
        /// [_id, _language, active, address, address-city, address-country, address-postalcode, address-state, address-use, 
        /// animal-breed, animal-species, birthdate, death-date, deceased, email, family, gender, general-practitioner, given, 
        /// identifier, language, link, name, organization, phone, phonetic, telecom]
        /// </summary>
        /// <param name="patient"></param>
        public static void DeletePatientBySearchParams(Patient patient)
        {
            try
            {
                LogToFile("Delete Patient");

                LogToFile("Request: ");
                var patientXml = FhirSerializer.SerializeResourceToXml(patient);
                LogToFile(XDocument.Parse(patientXml).ToString());

                FhirClient fhirClient = new FhirClient(FhirClientEndPoint);

                SearchParams searchParams = new SearchParams();
                searchParams.Add("_id", patient.Id);

                fhirClient.Delete("Patient", searchParams);

                LogToFile("Response: Delete has no response");
            }
            catch (Exception ex)
            {
                LogToFile(ex.ToString());
            }
        }

        /// <summary>
        /// Current not used in the application
        /// </summary>
        /// <param name="given"></param>
        /// <param name="family"></param>
        /// <returns></returns>
        public static Bundle SearchPatient(string given, string family)
        {
            Bundle bundle = new Bundle();

            try
            {
                FhirClient fhirClient = new FhirClient(FhirClientEndPoint);
                bundle = fhirClient.Search<Patient>(new string[] { "family=Fhirman" }); //Note: SearchParams can also be used here
                //foreach (var Entry in bundle.Entry)
            }
            catch(Exception ex)
            {
                LogToFile(ex.ToString());
            }

            return bundle;
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
