namespace SampleImplementation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using mailworxAPI;
   
	/// <summary>
	/// This class will show you how a campaign can be created and updated in mailworx.
	/// </summary>
	public class CampaignCreator {
		#region Constants
		const string CAMPAIGN_NAME = "mailworx campaign";
		#endregion

		#region Variables
		MailworxWebServiceAgent serviceAgent;
		SecurityContext securityContext;
		#endregion

		#region Constructor
		public CampaignCreator(MailworxWebServiceAgent serviceAgent, SecurityContext securityContext) {
			if (serviceAgent == null)
				throw new ArgumentNullException("serviceAgent", "serviceAgent must not be null!");
			if (serviceAgent == null)
				throw new ArgumentNullException("securityContext", "securityContext must not be null!");

			this.serviceAgent = serviceAgent;
			this.securityContext = securityContext;
		}
		#endregion

		#region CreateCampaign
		/// <summary>
		/// Creates a campaign in mailworx.
		/// </summary>
		/// <param name="profileId">The profile id that should be used for the campaign.</param>
		/// <returns>Returns a KeyValuePair where the key is the template id and the value is the created campaign id.</returns>
		/// <exception cref="System.ArgumentException">profileId must not be an empty guid!</exception>
		public KeyValuePair<Guid, Guid> CreateCampaign(Guid profileId) {
			if (profileId == Guid.Empty)
				throw new ArgumentException("profileId", "profileId must not be an empty guid!");

			// Load the original campaign.
			Campaign originalCampaign = this.LoadCampaign();

			if (originalCampaign != null) {
				if (originalCampaign.Name.Equals(CAMPAIGN_NAME)) {
					
					// Copy the original campaign
					Campaign copyCampaign = this.CopyCampaign(originalCampaign.Guid);

					// Update the sender, profile, ....
					if (this.UpdateCampaign(copyCampaign, profileId)) {
						return new KeyValuePair<Guid, Guid>(copyCampaign.Guid, copyCampaign.TemplateGuid);
					}
				}
				else {
					// Return the already existing campaign.
					return new KeyValuePair<Guid, Guid>(originalCampaign.Guid, originalCampaign.TemplateGuid);
				}
			}

			return new KeyValuePair<Guid, Guid>();
		}
		#endregion

		#region Private Methods
		#region LoadCampaign
		/// <summary>
		/// Loads the campaign with the specified id.
		/// </summary>
		/// <param name="campaignId">The campaign id.</param>
		/// <returns>Returns the campaign.</returns>
		private Campaign LoadCampaign(Guid? campaignId = null) {
			// Build up the request.
			CampaignsRequest campaignRequest = new CampaignsRequest() {
				SecurityContext = this.securityContext,
				Type = CampaignType.InWork,
				Language = "EN"
			};

			if (campaignId.HasValue) { // If there is a campaign id given, then load the campaign by its id.

				campaignRequest.Id = campaignId.Value;

				CampaignsResponse response = this.serviceAgent.GetCampaigns(campaignRequest);

				if (response == null)
					return null;
				else
					return response.Campaigns.FirstOrDefault();
			}
			else { // If there is no campaign id given, then load the campaign by its name.

				Campaign[] loadedCampaigns = this.serviceAgent.GetCampaigns(campaignRequest).Campaigns;
				Campaign existingCampaign = loadedCampaigns.FirstOrDefault(c => c.Name.Equals("My first campaign", StringComparison.OrdinalIgnoreCase));

				if (existingCampaign == null) {
					return loadedCampaigns.FirstOrDefault(c => c.Name.Equals(CAMPAIGN_NAME, StringComparison.OrdinalIgnoreCase));
				}
				else {
					return existingCampaign;
				}
			}
		}
		#endregion

		#region CopyCampaign
		private Campaign CopyCampaign(Guid campaignId) {
			CopyCampaignRequest copyCampaignRequest = new CopyCampaignRequest() {
				Language = "EN",
				SecurityContext = this.securityContext,
				CampaignToCopy = campaignId // The campaign which should be copied.
			};

			CopyCampaignResponse copyCampaignResponse = this.serviceAgent.CopyCampaign(copyCampaignRequest);

			if (copyCampaignResponse != null) {
				return this.LoadCampaign(copyCampaignResponse.NewCampaignGuid);
			}
			else {
				return null;
			}
		}
		#endregion

		#region UpdateCampaign
		private bool UpdateCampaign(Campaign campaignToUpdate, Guid profileId) {
			// Every value of type string in the UpdateCampaignRequest must be assigned, otherwise it will be updated to the default value (which is string.Empty).

			UpdateCampaignRequest updateRequest = new UpdateCampaignRequest() {
				CampaignGuid = campaignToUpdate.Guid,
				Language = campaignToUpdate.Culture,
				SecurityContext = this.securityContext,
				ProfileGuid = profileId,
				Name = "My first campaign",
				SenderAddress = "service@mailworx.info",
				SenderName = "mailworx Service Crew",
				Subject = "My first Newsletter"
			};

			return this.serviceAgent.UpdateCampaign(updateRequest) != null;
		}
		#endregion
		#endregion
	}
}