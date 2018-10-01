using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaultTools.vault
{
    class PropDefEnum
    {
        /*
1, SysName=VisualizationCompliance, DispName=Visualization Compliance, DataType=Numeric
2, SysName=Status, DispName=Status, DataType=String
3, SysName=Classification, DispName=Classification, DataType=String
4, SysName=VersionNumber, DispName=Version, DataType=Numeric
5, SysName=Comment, DispName=Comment, DataType=String
6, SysName=NumManualAttachments, DispName=Number of Attachments, DataType=Numeric
7, SysName=DateVersionCreated, DispName=Date Version Created, DataType=DateTime
8, SysName=CreateUserName, DispName=Created By, DataType=String
9, SysName=CheckInDate, DispName=Checked In, DataType=DateTime
10, SysName=ClientFileName, DispName=File Name, DataType=String
11, SysName=ModDate, DispName=Date Modified, DataType=DateTime
12, SysName=FileSize, DispName=File Size, DataType=Numeric
13, SysName=CheckoutLocalSpec, DispName=Checked Out Local Spec, DataType=String
14, SysName=CheckoutMachine, DispName=Checked Out Machine, DataType=String
15, SysName=CheckoutUserName, DispName=Checked Out By, DataType=String
16, SysName=Hidden, DispName=Hidden, DataType=Bool
17, SysName=LatestVersion, DispName=Latest Version, DataType=Bool
18, SysName=Originator, DispName=Originator, DataType=String
19, SysName=OrigCreateDate, DispName=Original Create Date, DataType=DateTime
20, SysName=Thumbnail, DispName=Thumbnail, DataType=Image
21, SysName=Provider, DispName=Provider, DataType=String
22, SysName=UserStatus, DispName=User Status, DataType=String
23, SysName=File Size01-{E81E6167-363D-4422-B12B-EA22B2BD6315}, DispName=File Size01, DataType=Numeric
24, SysName=Last Write-{8360E871-2E42-48FB-AF06-4349965DE7C1}, DispName=Last Write, DataType=DateTime
25, SysName=Date Created-{CA7C111A-00C9-4697-B06C-46043E0121C7}, DispName=Date Created, DataType=DateTime
26, SysName=Title, DispName=Title, DataType=String
27, SysName=Subject, DispName=Subject, DataType=String
28, SysName=Author, DispName=Author, DataType=String
29, SysName=Keywords, DispName=Keywords, DataType=String
30, SysName=Comments, DispName=Comments, DataType=String
31, SysName=Template-{22F00F28-FF56-4C40-90F6-910FAA4E6014}, DispName=Template, DataType=String
32, SysName=Last Author-{B8EC999F-0765-458A-AEE3-80B9AB41D353}, DispName=Last Author, DataType=String
33, SysName=RevNumber, DispName=Rev Number, DataType=String
34, SysName=Edit Time-{8D23971E-F5F7-4030-AC5F-3EEA86B6F988}, DispName=Edit Time, DataType=DateTime
35, SysName=Last Printed-{F36820E8-CB92-4340-96AC-2C9E7C094D69}, DispName=Last Printed, DataType=DateTime
36, SysName=Created Time-{C0C73F20-8C9B-4AB3-ACF5-EE99F90BD888}, DispName=Created Time, DataType=DateTime
37, SysName=Last Saved Time-{D37CD065-9419-46B5-8EF2-1107210FB780}, DispName=Last Saved Time, DataType=DateTime
38, SysName=Page Count-{9D996AAB-E5C5-4FA8-8DB1-22DD83788D66}, DispName=Page Count, DataType=Numeric
39, SysName=Word Count-{E90BAC1C-2AD3-457B-8BA1-48684A7150B8}, DispName=Word Count, DataType=Numeric
40, SysName=Char Count-{42391C20-84BD-4A83-B22A-6AEDD54626CC}, DispName=Char Count, DataType=Numeric
41, SysName=App Name-{D6A59332-4029-4A2D-B0B0-37C7F3AD62D1}, DispName=App Name, DataType=String
42, SysName=Security-{6333E663-DC55-43FB-92F6-8F2AE8E07E7B}, DispName=Security, DataType=Numeric
43, SysName=Category-{A6FB732B-2755-4EB1-B312-333166574551}, DispName=Category, DataType=String
44, SysName=Presentation Target-{8622429C-52CA-450F-A9D8-A2F3BC63321A}, DispName=Presentation Target, DataType=String
45, SysName=Byte Count-{A8AA87A8-3389-47E3-869E-0A556B5175F6}, DispName=Byte Count, DataType=Numeric
46, SysName=Line Count-{52FCF0BA-78F0-40E9-873E-23EBB9BFF19B}, DispName=Line Count, DataType=Numeric
47, SysName=Para Count-{592DC51F-DD9A-44E7-9238-304AD80D72BB}, DispName=Para Count, DataType=Numeric
48, SysName=Slide Count-{A173B718-DA08-4C90-8AFE-28065670DA5E}, DispName=Slide Count, DataType=Numeric
49, SysName=Note Count-{A24AE7C2-FB21-462C-8F2B-994EA87CC115}, DispName=Note Count, DataType=Numeric
50, SysName=Hidden Count-{456CA8F6-356A-4644-AF49-53BEF103009B}, DispName=Hidden Count, DataType=Numeric
51, SysName=Media Clip Count-{C37B081A-AAAC-4842-BAC4-4D6E7CAFFD3E}, DispName=Media Clip Count, DataType=Numeric
52, SysName=Scale-{07C05F5A-BC2A-4E8A-8B08-BF692BCABB17}, DispName=Scale, DataType=Bool
53, SysName=Manager-{4675F00F-5756-41CF-96F2-A996B7CBF9FA}, DispName=Manager, DataType=String
54, SysName=Company, DispName=Company, DataType=String
55, SysName=Links Dirty-{FDA92F37-03FD-4AF7-A562-C677D306A7C6}, DispName=Links Dirty, DataType=Bool
56, SysName=Editor-{3CD76FE7-5CF5-4FDA-9F87-BEAADECFF530}, DispName=Editor, DataType=String
57, SysName=Media Supplier-{98DFC813-E10C-4389-A57D-A6269514A1D6}, DispName=Media Supplier, DataType=String
58, SysName=Media Source-{9C0002EF-9AA4-45C3-8E45-9651DD8FE4F2}, DispName=Media Source, DataType=String
59, SysName=Media Sequence Number-{E54A4A84-EA6C-4A75-AF03-1371B6ADB120}, DispName=Media Sequence Number, DataType=String
60, SysName=Media Project-{F598B7AD-188E-4DCF-8677-83F929F6E541}, DispName=Media Project, DataType=String
61, SysName=Media Status-{49AF34C5-5E3F-4FBB-963B-C3CBD8E15B5B}, DispName=Media Status, DataType=Numeric
62, SysName=Media Owner-{A7CB1EA7-B008-4662-9FD5-A41E4C269F99}, DispName=Media Owner, DataType=String
63, SysName=Media Rating-{7D3E5421-57E9-4FEF-A5FB-0707CD2599A3}, DispName=Media Rating, DataType=String
64, SysName=Media Production-{08834AFB-A83D-4FA6-94B8-D6C32D5CBFD7}, DispName=Media Production, DataType=DateTime
65, SysName=Media Copyright-{AF6881FC-2939-4584-96C6-53DA87320A77}, DispName=Media Copyright, DataType=String
66, SysName=Doc Type-{9FD0432E-7422-4E30-B7DD-5E0944F413F2}, DispName=Doc Type, DataType=String
67, SysName=Authority-{A23C2D5E-EB13-4786-A5D3-4B7D15BEA3DB}, DispName=Authority, DataType=String
68, SysName=Web Link-{7699B93E-27E8-4C5B-A99B-199661E7C0DA}, DispName=Web Link, DataType=String
69, SysName=CheckedBy, DispName=Checked By, DataType=String
70, SysName=CostCenter, DispName=Cost Center, DataType=String
71, SysName=CreationDate, DispName=Date File Created, DataType=DateTime
72, SysName=Date Checked-{0A72A6F3-820E-4F4D-9B96-582E709B2DF4}, DispName=Date Checked, DataType=DateTime
73, SysName=Description, DispName=Description, DataType=String
74, SysName=Design Status-{4D877B8E-1EDF-48B9-B6BC-68EA30BBAA9E}, DispName=Design Status, DataType=String
75, SysName=Designer-{BF918969-F9C2-47DF-93E8-5AC9C531E2E3}, DispName=Designer, DataType=String
76, SysName=Doc Sub Type-{18F13CF6-4E72-4230-B96B-D4BC53E5E467}, DispName=Doc Sub Type, DataType=String
77, SysName=Doc Sub Type Name-{5A493546-7382-434F-AB4F-003F7811333E}, DispName=Doc Sub Type Name, DataType=String
78, SysName=Engineer-{C3DF0B83-932B-4B63-BF7D-730C730E19FB}, DispName=Engineer, DataType=String
79, SysName=Engr Approved By-{07E77B7A-1124-410F-8D7C-23E06778A827}, DispName=Engr Approved By, DataType=String
80, SysName=Date Engr Approved-{1467A198-A7CF-4C27-82EF-C8DF7C9A93F7}, DispName=Date Engr Approved, DataType=DateTime
81, SysName=External Property Revision-{71F9A929-B943-456C-829E-AAD852A1, DispName=External Property Revision, DataType=String
82, SysName=Manufacturer-{23D09E85-A81D-4D56-829B-A3FF536F0788}, DispName=Manufacturer, DataType=String
83, SysName=Material, DispName=Material, DataType=String
84, SysName=Mfg Approved By-{F544DC27-C037-4B45-8967-B15C933A6E25}, DispName=Mfg Approved By, DataType=String
85, SysName=Mfg Date Approved-{FD9F9C88-BB10-4B1A-8753-5363DAAA5A86}, DispName=Mfg Date Approved, DataType=DateTime
86, SysName=PartNumber, DispName=Part Number, DataType=String
87, SysName=Part Property Revision-{DD36BD46-6556-4AC5-AB6F-28F53F2F9EE5, DispName=Part Property Revision, DataType=String
88, SysName=Project-{8A94D1D4-E65B-4772-A677-92331D28CC4F}, DispName=Project, DataType=String
89, SysName=Standards Organization-{A47976C7-B377-4AB2-8A88-A7F5C1418AC3, DispName=Standards Organization, DataType=String
90, SysName=Vendor-{AE291C3B-61A3-495B-A872-86005917C75A}, DispName=Vendor, DataType=String
91, SysName=Revision Id-{4C65E173-1CB3-4B03-A441-9487B4E1B3D9}, DispName=Revision Id, DataType=String
92, SysName=Database Id-{9EF1D3E5-6947-485E-833C-A4CB8151BD25}, DispName=Database Id, DataType=String
93, SysName=Part Icon-{F470C8EE-A1A3-4921-BAAA-411FFFCCBC91}, DispName=Part Icon, DataType=Image
94, SysName=Parameterized Template-{D34A6379-8ED4-4C9D-A178-988DC2BF9967, DispName=Parameterized Template, DataType=Bool
95, SysName=SaveDate-{65215CBB-3E33-4011-81B7-499504D64645}, DispName=SaveDate, DataType=String
96, SysName=SaveTime-{99C2FFA2-CBB0-4BF4-ADE4-B5C608F0ABF9}, DispName=SaveTime, DataType=String
97, SysName=FileName-{1BB04693-5AA4-4F41-9C3C-9E807E774AAB}, DispName=FileName, DataType=String
98, SysName=TotalMass-{A2644BE1-F601-45F1-88E2-252B85557870}, DispName=TotalMass, DataType=String
99, SysName=TotalVolume-{DF6BD00D-D244-4995-B892-E3AE55AA826E}, DispName=TotalVolume, DataType=String
100, SysName=FirstViewScale-{A358D80C-23B8-4EA7-88A3-BC40D6A417D7}, DispName=FirstViewScale, DataType=String
101, SysName=AllViewScales-{3572FDE7-1C88-4AF1-8EC9-C72FFC498B18}, DispName=AllViewScales, DataType=String
102, SysName=FirstViewScale:1-{4738C7FE-EC68-40DF-A771-76EA99BFC4C9}, DispName=FirstViewScale:1, DataType=String
103, SysName=UNIDADE-{EA004DF1-45D6-427D-A520-8FEFFD6B7C8B}, DispName=UNIDADE, DataType=String
104, SysName=PaperSize-{8BBCA654-E8C1-4E0D-AF7D-FE984CAFA03A}, DispName=PaperSize, DataType=String
105, SysName=MajorVersion-{A88EA35D-ACE6-482B-BEB4-10AD1F666233}, DispName=MajorVersion, DataType=Numeric
106, SysName=MinorVersion-{EEFB9B77-9827-41C7-BA1F-CFDC6C02264A}, DispName=MinorVersion, DataType=Numeric
107, SysName=AmdtFileType-{4BCFC9C8-CD86-46A1-AD49-8A254CE1A4BA}, DispName=AmdtFileType, DataType=String
108, SysName=StyleSheet-{B6BFEFE3-FCBB-4B83-9443-7202FAF505D0}, DispName=StyleSheet, DataType=String
109, SysName=Menu-{C3F1580A-DFB3-4839-ADDB-88EB959244C1}, DispName=Menu, DataType=String
110, SysName=Dimblk-{58B6A533-EF89-4126-9387-048F5B9DB070}, DispName=Dimblk, DataType=String
111, SysName=Dimapost-{B46F5CC5-C51A-403D-981E-D00BFDB2E08D}, DispName=Dimapost, DataType=String
112, SysName=SM_Thickness-{741E9F01-181B-4BA6-AD1B-6CB57F34C43A}, DispName=SM_Thickness, DataType=String
113, SysName=SM_Style-{5CBBE43C-2D70-4C3C-B92F-8FFA3DF4AE89}, DispName=SM_Style, DataType=String
114, SysName=SM_Extents-{20C573AD-ED78-412D-9BE9-E1D033619899}, DispName=SM_Extents, DataType=String
115, SysName=SM_Square-{0801EE7F-65D9-46B3-AF91-6B7D63145441}, DispName=SM_Square, DataType=String
116, SysName=SM_Extents_Width-{3DD4EFA9-5C0E-456A-82C7-8F4E6B0D734E}, DispName=SM_Extents_Width, DataType=String
117, SysName=SM_Extents_Length-{4E0E907D-80A4-4602-831C-7E6761984535}, DispName=SM_Extents_Length, DataType=String
118, SysName=StockNumber, DispName=Stock Number, DataType=String
119, SysName=Dimblk1-{A48BB912-E55B-467C-AEE8-53913DDC2376}, DispName=Dimblk1, DataType=String
120, SysName=d1-{9D43F0E6-476D-4101-B1D2-6D0CF6135F6B}, DispName=d1, DataType=String
121, SysName=d51-{C463A97A-A5CD-4830-96D5-2B8FCBAE799C}, DispName=d51, DataType=String
122, SysName=RenderElements-{5C3889CF-BCB7-4D7C-94C4-D8D7CAE1829F}, DispName=RenderElements, DataType=Numeric
123, SysName=Scene Flags-{47DA03D0-2733-4D84-BD5F-A22DD5AAC483}, DispName=Scene Flags, DataType=Numeric
124, SysName=Render Flags-{0B91F074-C448-4BED-8F86-F51FD3234A5A}, DispName=Render Flags, DataType=Numeric
125, SysName=Animation End-{410C18AC-B97B-4BD3-9163-8D202991C9DD}, DispName=Animation End, DataType=Numeric
126, SysName=Animation Start-{242FA32A-0A2D-49E2-B7E6-C0F4F727FD77}, DispName=Animation Start, DataType=Numeric
127, SysName=Renderer Name-{2F7FF463-0420-482D-87A3-38658B66FB55}, DispName=Renderer Name, DataType=String
128, SysName=Renderer ClassIDB-{BDF5EA40-CE39-475B-92A2-0BA726943C1C}, DispName=Renderer ClassIDB, DataType=Numeric
129, SysName=Renderer ClassIDA-{82BC9177-1D3B-4391-8580-C301D8C18A47}, DispName=Renderer ClassIDA, DataType=Numeric
130, SysName=Render Aspect-{1008C0FF-406C-42D1-9AED-251EFE38E3D4}, DispName=Render Aspect, DataType=Numeric
131, SysName=Render Height-{62CF6EE1-C229-4EB4-A075-8500B0D15F2B}, DispName=Render Height, DataType=Numeric
132, SysName=Render Width-{CF64F65D-8C2B-4A46-B30C-A0408B4C241E}, DispName=Render Width, DataType=Numeric
133, SysName=Computer Name-{CCC68157-74CC-4723-9C3F-4B134E8ABABA}, DispName=Computer Name, DataType=String
134, SysName=User Name-{7CBDA4A0-D483-4D58-8EF2-77CD533CCD18}, DispName=User Name, DataType=String
135, SysName=Used Plug-Ins-{0BF990AB-8ED6-486C-AAF9-C40A346897F6}, DispName=Used Plug-Ins, DataType=String
136, SysName=Materials-{1FCE51C2-1BD5-4B7A-84EF-E169C0427A1B}, DispName=Materials, DataType=String
137, SysName=Objects-{96A427FD-150B-4BF5-B14C-252B6A5D6BFE}, DispName=Objects, DataType=String
138, SysName=External Dependencies-{3748DDDB-C481-4AC4-899F-E20E161399FA}, DispName=External Dependencies, DataType=String
139, SysName=Scene Total-{DB854BA3-8922-4112-ABA9-C3B24611DD10}, DispName=Scene Total, DataType=Numeric
140, SysName=Scene Space Warps-{B7C83BD8-BAB2-4B95-806B-F808737EED98}, DispName=Scene Space Warps, DataType=Numeric
141, SysName=Scene Helpers-{0FDD2B3A-8BF1-44E0-BF1D-6FAE65F46245}, DispName=Scene Helpers, DataType=Numeric
142, SysName=Scene Cameras-{81529174-230B-4DE1-AC3B-4B86C7FFC2EF}, DispName=Scene Cameras, DataType=Numeric
143, SysName=Scene Lights-{CBB6248F-2036-4F02-A7FD-68C03BA85040}, DispName=Scene Lights, DataType=Numeric
144, SysName=Scene Shapes-{C9456A79-FD45-4CAE-9412-B014999AA58A}, DispName=Scene Shapes, DataType=Numeric
145, SysName=Scene Objects-{D7378682-CC4B-4920-BECB-C8A472A0318A}, DispName=Scene Objects, DataType=Numeric
146, SysName=Mesh Faces-{722DB4B1-65E0-4965-9F58-F815E50C8470}, DispName=Mesh Faces, DataType=Numeric
147, SysName=Mesh Vertices-{4B34BF33-02DE-453A-93EB-6E7DECB77470}, DispName=Mesh Vertices, DataType=Numeric
148, SysName=Compressed-{8406E357-2859-4306-8F44-D842E3E48BAB}, DispName=Compressed, DataType=Bool
149, SysName=3ds Max Version-{0D5C6E72-5B3A-4F03-AA6B-45CDF330EC1B}, DispName=3ds Max Version, DataType=Numeric
150, SysName=Proxy Refresh Date-{F17AE5C0-4645-4D02-861C-5C7ECD371B0C}, DispName=Proxy Refresh Date, DataType=DateTime
151, SysName=Standard-{D42EE91A-6794-4037-9517-00AC71719FE1}, DispName=Standard, DataType=String
152, SysName=External Property Revision Id-{0A7770E5-5941-4881-B36C-CFD7F, DispName=External Property Revision Id, DataType=String
153, SysName=Language-{B9C6B17B-AF90-4520-83A9-733E9A930617}, DispName=Language, DataType=String
154, SysName=L-{71C33446-AE24-4200-8852-6B4D07B6ABFD}, DispName=L, DataType=String
155, SysName=PL-{290C59CE-31EC-4E8A-AC79-A3F50173D1E1}, DispName=PL, DataType=String
156, SysName=RPL-{8698B5F0-558D-4F1A-82E2-B1C79EE76DE2}, DispName=RPL, DataType=String
157, SysName=OPL-{85AD8257-167B-41DB-844B-878985A6F9D5}, DispName=OPL, DataType=String
158, SysName=ROPL-{E1AF0FA2-9D17-425C-9636-3C5D8BE06A22}, DispName=ROPL, DataType=String
159, SysName=Translation Standard-{2779F2F2-65C8-4D9C-9568-A7885353E38D}, DispName=Translation Standard, DataType=String
160, SysName=File Name01-{E4EAD248-B7D2-4C65-88AF-1D13478D4025}, DispName=File Name01, DataType=String
161, SysName=Preprocessor-{1B6F6248-42B2-4AD3-B02E-AC9BC81D9364}, DispName=Preprocessor, DataType=String
162, SysName=Postprocessor-{54BB824D-B241-420B-917C-51FF31E66565}, DispName=Postprocessor, DataType=String
163, SysName=Sent Units-{FB403E64-3D70-44E8-B72B-E591A09FB7E0}, DispName=Sent Units, DataType=String
164, SysName=Uncertainty-{9E7B27CF-AAFA-42CA-A798-52BD07613DA4}, DispName=Uncertainty, DataType=Numeric
165, SysName=Data-{F5879359-4D57-46D6-8265-70323B220B5C}, DispName=Data, DataType=String
166, SysName=Desenho-{DE942488-C3D4-4F77-86E2-ADCDC597083B}, DispName=Desenho, DataType=String
167, SysName=Projeto-{1674EE33-F662-4A09-A17E-E4CA3F90BEC4}, DispName=Projeto, DataType=String
168, SysName=RevisÆo-{65F91A3B-D350-41E4-8D4C-2D73B3C22043}, DispName=RevisÆo, DataType=String
169, SysName=Escala-{89F2141A-5F5D-463C-AB5F-9ABBE7D01932}, DispName=Escala, DataType=String
170, SysName=Unidades-{527945CF-F2DD-41FB-BE5B-9E5688652982}, DispName=Unidades, DataType=String
171, SysName=IsCustomPart-{1E735F9F-A177-4D76-A08C-02513160A551}, DispName=IsCustomPart, DataType=String
172, SysName=Standard Revision-{341C9EA9-E844-434A-A475-89BCEEE784D3}, DispName=Standard Revision, DataType=String
173, SysName=FirstViewScale:2-{B0F61D34-BAB2-447A-9E7E-CF0DFE9D78E2}, DispName=FirstViewScale:2, DataType=String
174, SysName=Part Property Revision Id-{31731467-9323-4D21-AC39-D7AC6A66C, DispName=Part Property Revision Id, DataType=String
175, SysName=FirstViewScale:3-{353AAEE8-44C8-448A-AB09-C2A7829F193D}, DispName=FirstViewScale:3, DataType=String
176, SysName=Date FDA-{F07F1C1C-920E-4FB0-B251-323A308EFE44}, DispName=Date FDA, DataType=String
177, SysName=DWGCreatorName, DispName=DWG Creator Name, DataType=String
178, SysName=DWGCreatorVersion, DispName=DWG Creator Version, DataType=String
179, SysName=ClientFileName(Ver), DispName=File Name (Historical), DataType=String
180, SysName=CheckoutDate, DispName=Checked Out, DataType=DateTime
181, SysName=iLogicRuleStatus, DispName=iLogicRuleStatus, DataType=String
183, SysName=Name, DispName=Name, DataType=String
186, SysName=Extension, DispName=File Extension, DataType=String
         */
    }
}
