namespace SampleImplementation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using mailworxAPI;

	/// <summary>
	/// This class will show you how subscribers can be imported into mailworx.
	/// </summary>
	public class SubscriberImport {
		#region Constants
		const string PROFILE_NAME = "MyFirstProfile";
		#endregion

		#region Variables
		MailworxWebServiceAgent serviceAgent;
		SecurityContext securityContext;
		#endregion

		#region Constructor
		public SubscriberImport(MailworxWebServiceAgent serviceAgent, SecurityContext securityContext) {
			if (serviceAgent == null)
				throw new ArgumentNullException("serviceAgent", "serviceAgent must not be null!");
			if (serviceAgent == null)
				throw new ArgumentNullException("securityContext", "securityContext must not be null!");

			this.serviceAgent = serviceAgent;
			this.securityContext = securityContext;
		}
		#endregion

		#region ImportSubscribers
		/// <summary>
		/// Imports the subscribers.
		/// </summary>
		/// <returns>Returns a KeyValuePair. The key is the profile id and the value is a list of ids of the imported subscribers.</returns>
		public KeyValuePair<Guid, List<Guid>> ImportSubscribers() {
			
			// Create a new import request.
			SubscriberImportRequest importRequest = new SubscriberImportRequest();



			// ### HANDLE PROFILE ###
			// Here we handle the profile that will be used as target group later.

			// Load the profile with the given name from mailworx.
			Profile profile = this.LoadProfile(PROFILE_NAME);

			// If there is already a profile for the given name, all subscribers of this group have to be removed.
			if (profile != null) {
				// This action will take place before the import has started.
				ClearProfileAction clearProfile = new ClearProfileAction() { Name = profile.Name };
				importRequest.BeforeImportActions = new BeforeImportAction[] { clearProfile };
			}

			// This action will take place after the subscribers have been imported to mailworx.
			ProfileAdderAction addToProfile = new ProfileAdderAction() {
				Name = PROFILE_NAME, // A new profile will be created if no profile does exist for the given name in mailworx.
				// ExecuteWith.Insert -> Only subscribers which will be added as new subscribers will be assigned to the profile.
				// ExecuteWith.Update -> Only subscribers which already exist will be assigned to the profile.
				// ExecuteWith.Insert | ExecuteWith.Update -> Every imported subscriber will be assigned to the profile.
				ExecuteWith = ExecuteWith.Insert | ExecuteWith.Update
			};
			importRequest.PostSubscriberActions = new PostSubscriberAction[] { addToProfile };

			// ### HANDLE PROFILE ###



			// ### HANDLE LIST OF SUBSCRIBERS ###

			// Get some sample subscribers to import.
			importRequest.Subscribers = this.GetSubscribers();

			// ### HANDLE LIST OF SUBSCRIBERS ###



			// ### HANDLE IMPORT PROPERTIES ###

			// Usually we use the email address as duplicate criteria.
			// But you can also use the internal name of another custom field here, e.g. "crmid".
			importRequest.DuplicateCriteria = "email";
			importRequest.Language = "EN";

			// ### HANDLE IMPORT PROPERTIES ###



			importRequest.SecurityContext = this.securityContext;

			List<Guid> importedSubscriberIds = new List<Guid>();



			// ### DO THE IMPORT ###

			// Import the data by calling the webservice method.
			SubscriberImportResponse importResponse = this.serviceAgent.ImportSubscribers(importRequest);

			// ### DO THE IMPORT ###



			// ### HANDLE THE IMPORT RESPONSE ###

			// Here we use our console application in order to show you the results/errors of the import response.

			Console.WriteLine("-------------------------------Import result----------------------");
			Console.WriteLine(string.Format("    Duplicates: {0}", importResponse.Duplicates));
			Console.WriteLine(string.Format("    Erros: {0}", importResponse.Errors));
			Console.WriteLine(string.Format("    Imported: {0}", importResponse.Imported));
			Console.WriteLine(string.Format("    Updated: {0}", importResponse.Updated));

			if (importResponse.FeedbackData != null && importResponse.FeedbackData.Length > 0) {
				Console.WriteLine("    Feedback data:");
				for (int i = 0; i < importResponse.FeedbackData.Length; i++) {
					string text = string.Format("        {0}. ", i + 1);

					if (string.IsNullOrEmpty(importResponse.FeedbackData[i].Error)) {
						importedSubscriberIds.Add(importResponse.FeedbackData[i].AffectedSubscriber.Value);
						Console.WriteLine(string.Format("{0}Email {1}, Id: {2}", text, importResponse.FeedbackData[i].UniqueId, importResponse.FeedbackData[i].AffectedSubscriber.Value));
					}
					else {
						Console.WriteLine(string.Format("{0}{1}", text, importResponse.FeedbackData[i].Error));
					}
				}

				Console.WriteLine();
			}
			else {
				Console.WriteLine("    No feedback data");
			}

			Console.WriteLine("------------------------------------------------------------------");

			// If the profile did not exist at the the first iteration we can now load it.
			if (profile == null) {
				profile = this.LoadProfile(PROFILE_NAME);
			}

			// ### HANDLE THE IMPORT RESPONSE ###



			return new KeyValuePair<Guid, List<Guid>>(profile.Guid, importedSubscriberIds);
		}
		#endregion

		#region GetFieldsOfAccount
		/// <summary>
		/// Gets the subscriber fields of the account which has been set in the security context.
		/// </summary>
		/// <returns>Returns an array of subscriber fields for the given account.</returns>
		public Field[] GetFieldsOfAccount() {
						SubscriberFieldRequest fieldRequest = new SubscriberFieldRequest();
			fieldRequest.Language = "EN";
			fieldRequest.SecurityContext = this.securityContext;

			// MetaInformation -> Will return predefined fields like tel.nr., email, firstname, lastname, ...
			// CustomInformation -> Will return custom defined fields.
			// MetaInformation | CustomInformation -> Will return all kind of fields.
			fieldRequest.FieldType = FieldType.MetaInformation | FieldType.CustomInformation;



			SubscriberFieldResponse subscriberFieldResponse = null;
			try {
				subscriberFieldResponse = this.serviceAgent.GetSubscriberFields(fieldRequest);
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				throw ex;
			}



			if (subscriberFieldResponse != null) {
				Console.WriteLine("-------------------------------Fields----------------------");

				for (int i = 0; i < subscriberFieldResponse.Fields.Length; i++) {
					Type fieldType = subscriberFieldResponse.Fields[i].GetType();

					Console.WriteLine(string.Format("    +++++++++++++++ Field {0} +++++++++++++++    ", i + 1));
					Console.WriteLine(string.Format("    Type: {0}{1}    Internalname: {2}", fieldType.Name, Environment.NewLine, subscriberFieldResponse.Fields[i].InternalName));

					// If the field is of the seletion, the selection fields should also be displayed.
					if (fieldType == typeof(SelectionField)) {
						Console.WriteLine("    Selections: ");
						SelectionFieldElement[] selections = ((SelectionField)subscriberFieldResponse.Fields[i]).SelectionObjects;

						for (int j = 0; j < selections.Length; j++) {
							Console.WriteLine(string.Format("        {0}. {1}", j + 1, selections[j].InternalName));
						}
					}

					Console.WriteLine("    +++++++++++++++++++++++++++++++++++++++    ");

					if (i + 1 < subscriberFieldResponse.Fields.Length) {
						Console.WriteLine();
					}
				}

				Console.WriteLine("---------------------------------------------------------");
			}

			return subscriberFieldResponse.Fields;
		}
		#endregion

		#region GetSubscribers
		/// <summary>
		/// Get some sample subscriber for the import.
		/// </summary>
		/// <returns>Returns an array of subscribers.</returns>
		private Subscriber[] GetSubscribers() {

			// We build some new sample subscribers here.

			#region SubscriberDetailedExample
			// This is a new subscriber.
			// We set some meta data as well as some custom data for this subscriber.

			Subscriber subscriberDetailedExample = new Subscriber();

			#region SubscriberMetaData
			// Here we set some meta data fields for this subscriber.

			// Set the meta data field "OptIn". 
			// If set to true the subscriber will receive newsletters.
			subscriberDetailedExample.OptIn = true;

			// Set the meta data field "Mailformat". 
			// Multipart -> The subscriber will receive the newsletter as multipart format.
			// HTML -> The subscriber will receive the newsletter as HTML format. 
			// Text -> The subscriber will receive the newsletter as text format. 
			subscriberDetailedExample.Mailformat = Mailformat.Multipart;

			// Set the meta data field "Language".
			// This is the language of the subscriber.
			// If no value is specified here, the language of the security context will be used. 
			subscriberDetailedExample.Language = "EN";

			// Set the meta data field "Status".
			// ActiveIfManualInactive 
			// -> If the current state of the subscriber is manual inactive, it will be changed to active. 
			// -> If the current state of the subscriber is automatic inactive, it won't be changed to active.
			// InactiveIfActive 
			// -> If the state of the subscriber is active, it will be changed to manual inactive.
			// -> If the state of the subscriber is manual inactive or automatic inactive, it won't be changed to manual inactive.
			// The status automatic inactive can only be set by the mailworx system itself.
			// If you don't want to change the value of existing subscribers leave this value unassigned.
			subscriberDetailedExample.Status = SubscriberStatus.ActiveIfManualInactive;
			#endregion

			#region SubscriberCustomData
			// Here we set some custom data fields for this subscriber.

			// If you want to know which fields are available for your account, then call the following method: 
			this.GetFieldsOfAccount();

			/*
			 * Beware: The internal name and the concrete object type of the field has to match the configuration in mailworx.
			 */

			// We set some fields with different field types here, just to show how to do it right:
			subscriberDetailedExample.Fields = new Field[] {
				new TextField() { // A field with this internal name exists in every mailworx account.
					InternalName = "email",
					UntypedValue = "service@mailworx.info"
				},
				new TextField() { // A field with this internal name exists in every mailworx account.
					InternalName = "firstname",
					UntypedValue = "mailworx"
				},
				new TextField() { // A field with this internal name exists in every mailworx account.
				InternalName = "lastname",
					UntypedValue = "ServiceCrew"
				},
				new DateTimeField() { 
					InternalName = "birthdate",
					UntypedValue = DateTime.Now.ToString()
				},
				new TextField { // A field of the type memo in mailworx is also a textfield
					InternalName = "note",
					UntypedValue = "JustPutYourTextRightHere"
				},
				new SelectionField {
					InternalName = "interest",
					UntypedValue = "interest_politics, interest_economy"
					// You can use , or ; here to split the values.
					// White spaces don't matter either.
					// UntypedValue = "interest_politics;interest_economy"
				},
				new SelectionField {
					InternalName = "position",
					UntypedValue = "position_sales"
				}
			};
			#endregion
			#endregion

			#region SubscriberExample
			Subscriber subscriberExample = new Subscriber();
			subscriberExample.Mailformat = Mailformat.Text;
			subscriberExample.OptIn = false;
			subscriberExample.Status = SubscriberStatus.Inactive;
			subscriberExample.Fields = new Field[] {
				new TextField {
					InternalName = "firstname",
					UntypedValue = "Max"
				},
				new TextField {
					InternalName = "lastname",
					UntypedValue = "Mustermann"
				},
				new NumberField() {
					InternalName = "customerid",
					UntypedValue = new Random().Next(1000, Int32.MaxValue).ToString()
				},
				new BooleanField() {
					InternalName = "iscustomer",
					UntypedValue = "true"
				},
				new TextField() {
					InternalName = "email",
					UntypedValue = "max@mustermann.at"
				}
			};
			#endregion

			#region SubscriberExample 2
			Subscriber subscriberExampleTwo = new Subscriber();
			subscriberExampleTwo.OptIn = true;
			subscriberExampleTwo.Language = "DE";
			subscriberExampleTwo.Mailformat = Mailformat.HTML;
			subscriberExampleTwo.Status = SubscriberStatus.Active;
			subscriberExampleTwo.Fields = new Field[] {
				new TextField() {
					InternalName = "lastname",
					UntypedValue = "Musterfrau"
				},
				new DateTimeField() {
					InternalName = "birthdate",
					UntypedValue = DateTime.Now.AddDays(-new Random().Next(20, 40)).ToString()
				},
				new SelectionField() {
					InternalName = "position",
					UntypedValue = "position_sales"
				},
				new BooleanField() {
					InternalName = "iscustomer",
					UntypedValue = Boolean.FalseString
				},
				new NumberField() {
					InternalName = "customerid",
					UntypedValue = "1"
				},
				 new TextField() {
					InternalName = "email",
					UntypedValue = "musterfrau@test.at"
				}
			};
			#endregion

			#region SubscriberExample 3
			Subscriber subscriberExampleThree = new Subscriber();
			subscriberExampleThree.OptIn = true;
			subscriberExampleThree.Language = "EN";
			subscriberExampleThree.Mailformat = Mailformat.HTML;
			subscriberExampleThree.Status = SubscriberStatus.Active;
			subscriberExampleThree.Fields = new Field[] {
				new TextField() {
					InternalName = "lastname",
					UntypedValue = "Musterfrau"
				},
				new DateTimeField() {
					InternalName = "birthdate",
					UntypedValue = DateTime.Now.AddDays(-new Random().Next(20, 40)).ToString()
				},
				new SelectionField() {
					InternalName = "position",
					UntypedValue = "position_sales;position_mechanic"
				},
				new BooleanField() {
					InternalName = "iscustomer",
					UntypedValue = Boolean.TrueString
				},
				new NumberField() {
					InternalName = "customerid",
					UntypedValue = string.Empty
				},
				new TextField() {
					InternalName = "email",
					UntypedValue = "isolde@musterfrau.at"
				}
			};
			#endregion

			return new Subscriber[] { subscriberDetailedExample, subscriberExample, subscriberExampleTwo, subscriberExampleThree };
		}
		#endregion

		#region LoadProfile
		/// <summary>
		/// Gets the profile with the specified profile name.
		/// </summary>
		/// <param name="profileName">The name of the profile to load.</param>
		/// <returns>The profile or null if the profile name was not found.</returns>
		private Profile LoadProfile(string profileName) {
			ProfilesRequest profileRequest = new ProfilesRequest() {
				Language = "EN",
				SecurityContext = this.securityContext,
				Type = ProfileType.Static // Only static profiles can be used for the subscriberimport.
			};

			try {
				ProfilesResponse profileResponse = this.serviceAgent.GetProfiles(profileRequest);

				if (profileResponse != null) {
					// Search the profile with the given name.
					return profileResponse.Profiles.FirstOrDefault(p => p.Name.Equals(profileName));
				}

				return null;
			}
			catch (Exception ex) {
				// A error occured
				Console.WriteLine(ex.ToString());
				throw ex;
			}
		}
		#endregion
	}
}
