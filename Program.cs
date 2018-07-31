/*
---------------------------------------------------------------------------------------------------------------------------------------------------
---  This is a sample implementation for using the mailworx API in order to create and send an email campaign over mailworx.                    ---
---  Be aware of the fact that this example might not work in your mailworx account.                                                            ---
---------------------------------------------------------------------------------------------------------------------------------------------------
---  ENSURE YOU PROVIDE YOUR CORRECT LOGIN DATA AT THE GetSecurityContext-Method                                                                ---
---------------------------------------------------------------------------------------------------------------------------------------------------
---																																				---
---  The following API methods get used in this example:                                                                                        ---
---     • GetProfiles                   http://www.mailworx.info/de/api/wunschsystem/beschreibung/getprofiles                                   ---
---     • GetSubscriberFields           http://www.mailworx.info/de/api/wunschsystem/beschreibung/getsubscriberfields                           ---
---     • ImportSubscribers             http://www.mailworx.info/de/api/wunschsystem/beschreibung/ImportSubscribers                             ---
---     • GetCampaigns                  http://www.mailworx.info/de/api/wunschsystem/beschreibung/GetCampaigns                                  ---
---     • CopyCampaign                  http://www.mailworx.info/de/api/wunschsystem/beschreibung/CopyCampaign                                  ---
---     • UpdateCampaign                http://www.mailworx.info/de/api/wunschsystem/beschreibung/UpdateCampaign                                ---
---     • GetSectionDefinitions         http://www.mailworx.info/de/api/wunschsystem/beschreibung/GetSectionDefinitions                         ---
---     • CreateSection                 http://www.mailworx.info/de/api/wunschsystem/beschreibung/CreateSection                                 ---
---     • SendCampaign                  http://www.mailworx.info/de/tour/e-mail/wunschsystem/beschreibung/versenden-einer-kampagne-sendcampaign ---
---                                                                                                                                             ---
---   This is a step by step example:                                                                                                           ---
---     1. Import the subscribers into mailworx                                                                                                 ---
---     2. Create a campaign                                                                                                                    ---
---     3. Add sections to the campaign                                                                                                         ---
---     4. Send the campaign to the imported subscribers                                                                                        ---
---------------------------------------------------------------------------------------------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using SampleImplementation.mailworxAPI;

namespace SampleImplementation {
	class Program {
		static void Main(string[] args) {



			// ### GENERAL ###

			// This is the agent which will be used as connection to the mailworx webservice.
			MailworxWebServiceAgent agent = new MailworxWebServiceAgent
			{

				// The url to the webservice.
				Url = "http://sys.mailworx.info/services/serviceagent.asmx"
			};

			// Set  the login data.
			SecurityContext context = GetSecurityContext();

			// ### GENERAL ###



			// ### STEP 1 : IMPORT ###
			// Here we use a helper class in order to do all the necessary import steps.
			SubscriberImport import = new SubscriberImport(agent, context);

			// The key is the id of the profile where the subscribers have been imported to.
			// The value is a list of ids of the imported subscribers.
			KeyValuePair<Guid, List<Guid>> importedData = import.ImportSubscribers();

			// ### STEP 1 : IMPORT ###


			
			// If some data where imported....
			if (importedData.Value != null && importedData.Value.Count > 0) {

				// ### STEP 2 : CREATE CAMPAIGN ###

				// Here we use another helper class in order to do all the necessary steps for creating a campaign.
				CampaignCreator campaignCreator = new CampaignCreator(agent, context);
				
				// The key is the id of the template.
				// The value is the id of the campaign.
				KeyValuePair<Guid, Guid> data = campaignCreator.CreateCampaign(importedData.Key);

				// ### STEP 2 : CREATE CAMPAIGN ###



				// If a campaign was returned we can add the sections.
				if (data.Key != Guid.Empty && data.Value != Guid.Empty) {

					// ### STEP 3 : ADD SECTIONS TO CAMPAIGN ###

					// Here we use another helper class in order to do all the necessary steps for adding sections to the campaign.
					SectionCreator sectionCreator = new SectionCreator(agent, context);

					// Send the campaign, if all sections have been created.
					if (sectionCreator.GenerateSection(data.Value, data.Key)) {

						// ### STEP 3 : ADD SECTIONS TO CAMPAIGN ###



						// ### STEP 4 : SEND CAMPAIGN ###

						SendCampaignRequest sendCampaignRequest = new SendCampaignRequest() {
							CampaignId = data.Key,
							IgnoreCulture = false, // Send the campaign only to subscribers with the same language as the campaign
							SecurityContext = context,
							SendType = CampaignSendType.Manual,
							Language = "EN",
							// If the SendType is set to Manual, ManualSendSettings are needed
							// If the SendType is set to ABSplit, ABSplitTestSendSettings are needed
							Settings = new ManualSendSettings() { SendTime = DateTime.Now },
							UseIRated = false, // Here is some more info about iRated http://www.mailworx.info/en/irated-technology
							UseRTR = true
						};

						// Send the campaign
						SendCampaignResponse sendCampaignResponse = agent.SendCampaign(sendCampaignRequest);

						// ### STEP 4 : SEND CAMPAIGN ###



						if (sendCampaignResponse == null) {
							Console.WriteLine("Something went wrong");
						}
						else {
							Console.WriteLine(string.Format("Effective subscribers: {0}", sendCampaignResponse.RecipientsEffective));
						}
					}
				}
			}

			Console.ReadLine();
		}

		private static SecurityContext GetSecurityContext() {
			return new SecurityContext() {
				Account = "[AccountName]", // The name of your mailworx account.
				Username = "[UserName]", // Your mailworx username.
				Password = "[Password]", // Your mailworx password.
				Source = "[SourceName]" // The name of your application which wants to access the mailworx webservice. 
				// You must register your application source at the following page, before you try to access the mailworx webservice: 
				// http://www.mailworx.info/de/api/api-schnittstelle-erstellen
			};
		}
	}
}
