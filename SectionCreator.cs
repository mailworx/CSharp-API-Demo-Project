namespace SampleImplementation {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using mailworxAPI;

	/// <summary>
	/// This class will show you how sections can be added to a campaign in mailworx.
	/// </summary>
	public class SectionCreator {
		#region Variables
		MailworxWebServiceAgent serviceAgent;
		SecurityContext securityContext;
		#endregion

		#region Constructor
		public SectionCreator(MailworxWebServiceAgent serviceAgent, SecurityContext securityContext) {
			if (serviceAgent == null)
				throw new ArgumentNullException("serviceAgent", "serviceAgent must not be null!");
			if (serviceAgent == null)
				throw new ArgumentNullException("securityContext", "securityContext must not be null!");

			this.serviceAgent = serviceAgent;
			this.securityContext = securityContext;
		}
		#endregion

		#region GenerateSection
		/// <summary>
		/// Generates the section for the given template into the given campaign.
		/// </summary>
		/// <param name="templateId">The template Id.</param>
		/// <param name="campaignId">The campaign Id.</param>
		/// <exception cref="System.ArgumentException">
		/// templateId;templateId must not be an empty guid!
		/// or
		/// campaignId;campaignId must not be an empty guid!
		/// </exception>
		public bool GenerateSection(Guid templateId, Guid campaignId) {
			if (templateId == Guid.Empty)
				throw new ArgumentException("templateId", "templateId must not be an empty guid!");
			if (campaignId == Guid.Empty)
				throw new ArgumentException("campaignId", "campaignId must not be an empty guid!");

			// Load all available section definitions for the given template
			SectionDefinition[] sectionDefinitions = this.LoadSectionDefinitions(templateId);
			bool sectionsCreated = sectionDefinitions != null && sectionDefinitions.Length > 0;

			// If there are no section definitions we can't setup the campaign.
			if (sectionDefinitions != null && sectionDefinitions.Length > 0) {
				// Right here we create three different sample sections for our sample campaign.

				#region SectionArticle
				// Load the section definition that defines an article.
				SectionDefinition definitionArticle = sectionDefinitions.FirstOrDefault(s => s.Name == "article");

				if (definitionArticle != null) {
					CreateSectionRequest createSectionRequest = new CreateSectionRequest() {
						Campaign = new Campaign { Guid = campaignId },
						SecurityContext = this.securityContext,
						Language = "EN",
					};



					// ### BUILD UP THE SECTION ###

					Section sectionArticle = new Section() {
						Created = DateTime.Now,
						SectionDefinitionName = definitionArticle.Name,
						StatisticName = "my first article"
					};

					/*
					 * Beware when setting field values: Please send new field-objects and ansure that the InternalName of the field contains the same value than the original field...
					 * The different field types use OO paradigms and define the kind of value in the field. Multi-Text-Line, Single-Text-Line, True/False settings etc. They are like datatypes in programming languages.
					 * The InternalName and the fieldtype has to match and they define at which field in the section the value will be entered.
					 * The fields are defined by the mailworx Template (=Layout of the email) and the defined fields there.
					 */ 
					List<Field> fieldsToAdd = new List<Field>();
					foreach (Field currentField in definitionArticle.Fields) {
						if (currentField.InternalName == "a_show") {
							fieldsToAdd.Add(new BooleanField() { InternalName = currentField.InternalName, UntypedValue = Boolean.TrueString });
						}
						else if (currentField.InternalName == "description") {
							fieldsToAdd.Add(new TextField() {
								InternalName = currentField.InternalName,
								// Beware single quotes do not work for attributes in HTML tags.
								// If you want to use double quotes for your text, you must use them HTML-encoded.
								// A text can only be linked with <a> tags and a href attributes. E.g.: <a href=""www.mailworx.info"">go to mailworx website</a>
								UntypedValue = @"Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy &quot;eirmod tempor&quot; invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. 
												At vero eos et accusam et <a href=""www.mailworx.info"">justo</a> duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. 
												Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. 
												At vero eos et accusam et justo duo dolores et ea rebum.  <a href=""http://sys.mailworx.info/sys/Form.aspx?frm=4bf54eb6-97a6-4f95-a803-5013f0c62b35"">Stet</a> clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet."
							});
						}
						else if (currentField.InternalName == "productimage") {
							// Upload the file from the given path to the mailworx media data base.
							Guid fileId = this.UploadFile(@"E:\mailworx\image_2016061694924427.png", "criteria.png");

							if (fileId != Guid.Empty) {
								fieldsToAdd.Add(new MdbField() { InternalName = currentField.InternalName, UntypedValue = fileId.ToString() });
							}
						}
						else if (currentField.InternalName == "name") {
							fieldsToAdd.Add(new TextField() { InternalName = currentField.InternalName, UntypedValue = "[%mwr:briefanrede%]" });
						}
					}

					sectionArticle.Fields = fieldsToAdd.ToArray();

					// ### BUILD UP THE SECTION ###



					createSectionRequest.Section = sectionArticle;


					// ### CREATE THE SECTION ###

					CreateSectionResponse response = this.serviceAgent.CreateSection(createSectionRequest);

					// ### CREATE THE SECTION ###



					sectionsCreated = sectionsCreated && response != null && response.Guid != Guid.Empty;
				}
				#endregion

				#region SectionBanner
				SectionDefinition definitionBanner = sectionDefinitions.FirstOrDefault(s => s.Name.Equals("banner"));
				if (definitionBanner != null) {
					CreateSectionRequest createBanner = new CreateSectionRequest() {
						Campaign = new Campaign { Guid = campaignId },
						SecurityContext = this.securityContext,
						Language = "EN"
					};

					Section banner = new Section() {
						Created = DateTime.Now,
						SectionDefinitionName = definitionBanner.Name,
						StatisticName = "banner"
					};

					List<Field> fieldsToAdd = new List<Field>();
					foreach (Field currentField in definitionBanner.Fields) {
						if (currentField.InternalName == "al_image") {
							Guid fileId = this.UploadFile(@"E:\mailworx\irated_header_final.jpg", "iratedHeader.jpg");

							if (fileId != Guid.Empty) {
								fieldsToAdd.Add(new MdbField() { InternalName = currentField.InternalName, UntypedValue = fileId.ToString() });
							}
						}
						else if (currentField.InternalName == "al_text") {
							fieldsToAdd.Add(
								new TextField() {
									InternalName = currentField.InternalName,
									UntypedValue = @"Developed in the <a href=""http://www.mailworx.info/en/"">mailworx</a> laboratory the intelligent and auto-adaptive algorithm <a href=""http://www.mailworx.info/en/irated-technology"">iRated®</a>
													 brings real progress to your email marketing. It is more than a target group oriented approach.
													 iRated® sorts the sections of your emails automatically depending on the current preferences of every single subscriber.
													 This helps you send individual emails even when you don't know much about the person behind the email address."
								});
						}
					}

					banner.Fields = fieldsToAdd.ToArray();
					createBanner.Section = banner;
					CreateSectionResponse response = this.serviceAgent.CreateSection(createBanner);
					sectionsCreated = sectionsCreated && response != null && response.Guid != Guid.Empty;
				}

				#endregion

				#region SectionTwoColumns
				SectionDefinition definitionTwoColumn = sectionDefinitions.FirstOrDefault(s => s.Name.Equals("section two columns"));
				if (definitionTwoColumn != null) {
					CreateSectionRequest createTwoColumn = new CreateSectionRequest() {
						Campaign = new Campaign { Guid = campaignId },
						SecurityContext = this.securityContext,
						Language = "EN"
					};

					Section twoColoumn = new Section() {
						Created = DateTime.Now,
						SectionDefinitionName = definitionTwoColumn.Name,
						StatisticName = "section with two columns"
					};

					List<Field> fieldsToAdd = new List<Field>();
					foreach (Field currentField in definitionTwoColumn.Fields) {
						if (currentField.InternalName == "atwo_left_image") {
							Guid fileId = this.UploadFile(@"E:\mailworx\connector.png", "connector.png");

							if (fileId != Guid.Empty) {
								fieldsToAdd.Add(new MdbField() { InternalName = currentField.InternalName, UntypedValue = fileId.ToString() });
							}
						}
						else if (currentField.InternalName == "atwo_left_text") {
							fieldsToAdd.Add(new TextField() { InternalName = currentField.InternalName, UntypedValue = @"Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat. 
																														Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim,
																														qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi." });
						}
						else if (currentField.InternalName == "atwo_right_image") {
							Guid fileId = this.UploadFile(@"E:\mailworx\event-app-qr-code-ticket.png", "event.png");

							if (fileId != Guid.Empty) {
								fieldsToAdd.Add(new MdbField() { InternalName = currentField.InternalName, UntypedValue = fileId.ToString() });
							}
						}
						else if (currentField.InternalName == "atwo_right_text") {
							fieldsToAdd.Add(new TextField() {
								InternalName = "atwo_right_text",
								UntypedValue = @"Nam liber tempor cum soluta nobis eleifend option congue nihil imperdiet doming id quod mazim placerat facer possim assum. Lorem ipsum dolor sit amet, consectetuer adipiscing elit,
											   sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat. Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo."
							});
						}
					}

					twoColoumn.Fields = fieldsToAdd.ToArray();
					createTwoColumn.Section = twoColoumn;
					CreateSectionResponse response = this.serviceAgent.CreateSection(createTwoColumn);
					sectionsCreated = sectionsCreated && response != null && response.Guid != Guid.Empty;
				}
				#endregion
			}

			return sectionsCreated;
		}
		#endregion

		#region LoadSectionDefinitions
		/// <summary>
		/// Loads the section definitions for the given template id.
		/// </summary>
		/// <param name="templateId">The template id.</param>
		/// <returns>Returns a array of section definitions for the given template.</returns>
		private SectionDefinition[] LoadSectionDefinitions(Guid templateId) {
			SectionDefinitionRequest sectionDefinitionRequest = new SectionDefinitionRequest() {
				Language = "EN",
				SecurityContext = this.securityContext,
				Template = new Template() { Guid = templateId }
			};

			SectionDefinitionResponse sectionDefinitionResponse = this.serviceAgent.GetSectionDefinitions(sectionDefinitionRequest);

			if (sectionDefinitionResponse == null)
				return null;
			else {



				// ### DEMONSTRATE SECTION DEFINITION STRUCTURE ###
				// Here we use the console application in order to demonstrate the structure of each section definition.
				// You need to know the structure in order to be able to create sections on your own.

				Console.WriteLine("-------------------------------Section definitions----------------------");

				for (int i = 0; i < sectionDefinitionResponse.SectionDefinitions.Length; i++) {
					Console.WriteLine(string.Format("    +++++++++++++++ Section definition {0} +++++++++++++++    ", i + 1));
					Console.WriteLine(string.Format("    Name:{0}", sectionDefinitionResponse.SectionDefinitions[i].Name));
					if (sectionDefinitionResponse.SectionDefinitions[i].Fields.Length > 0) {
						for (int j = 0; j < sectionDefinitionResponse.SectionDefinitions[i].Fields.Length; j++) {
							Field currentField = sectionDefinitionResponse.SectionDefinitions[i].Fields[j];
							Type currentFieldType = currentField.GetType();

							Console.WriteLine(string.Format("        *********** Field {0} ***********", j + 1));
							Console.WriteLine(string.Format("        Name: {0}", currentField.InternalName));
							Console.WriteLine(string.Format("        Type: {0}", currentFieldType.Name));

							if (currentFieldType == typeof(SelectionField)) {
								Console.WriteLine("                Selections:");

								for (int k = 0; k < ((SelectionField)currentField).SelectionObjects.Length; k++) {
									SelectionFieldElement selcField = ((SelectionField)currentField).SelectionObjects[k];
									Console.WriteLine(string.Format("                  Name:{0}", selcField.Caption));
									Console.WriteLine(string.Format("                  Value:{0}", selcField.InternalName));
								}

								Console.WriteLine($"        *****************************");
							}
						}
					}
					else {
						Console.WriteLine($"    No fields found");
					}

					Console.WriteLine("    +++++++++++++++++++++++++++++++++++++++    ");
				}

				Console.WriteLine("------------------------------------------------------------------------");

				// ### DEMONSTRATE SECTION DEFINITION STRUCTURE ###



				return sectionDefinitionResponse.SectionDefinitions;
			}
		}
		#endregion

		#region UploadFile
		/// <summary>
		/// Uploads a file to the mailworx media data base.
		/// </summary>
		/// <param name="path">The path where the file to upload is located.</param>
		/// <param name="fileName">Name of the file to upload.</param>
		/// <returns>Returns the id of the uploaded file.</returns>
		private Guid UploadFile(string path, string fileName) {
			// Get all files in the mdb for the directory mailworx.
			FileResponse fileResponse = this.serviceAgent.GetMDBFiles(new MediaDbRequest() { Language = "EN", Path = "mailworx", SecurityContext = this.securityContext });
			Guid fileId = Guid.Empty;

			// Check if there is already a file with the given filename.
			if (fileResponse == null || fileResponse.Files.FirstOrDefault(s => s.Name == fileName) == null) {
				// The file we want to upload.
				Byte[] picture = File.ReadAllBytes(path);

				// Send the data to mailworx
				FileUploadResponse uploadResponse = this.serviceAgent.UploadFileToMDB(new FileUploadRequest() {
					File = picture, // The picture as byte array.
					Language = "EN",
					Name = fileName, // The name of the file including the file extension.
					SecurityContext = this.securityContext,
					Path = "mailworx" // The location within the mailworx media database. If this path does not exist within the media data base, an exception will be thrown.
				});

				if (uploadResponse != null)
					fileId = uploadResponse.FileId;
			}
			else {
				fileId = fileResponse.Files.FirstOrDefault(s => s.Name == fileName).Id;
			}

			return fileId;
		}
		#endregion
	}
}
