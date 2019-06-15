using System;
using System.Collections.Generic;
using System.Text;

namespace RepairShoprToShipRush.Helpers
{
    public static class Constants
    {
        public const string xmlPayloadTemplate = "<?xml version = \"1.0\"?><Request><ShipTransaction><Shipment><Package><PackageActualWeight>0</PackageActualWeight></Package><DeliveryAddress><Address><FirstName>{0}</FirstName><Company>{1}</Company><Address1>{2}</Address1><Address2>{3}</Address2><City>{4}</City><State>{5}</State><Country>{6}</Country><StateAsString>{5}</StateAsString><CountryAsString>{6}</CountryAsString><PostalCode>{7}</PostalCode><Phone>{8}</Phone><EMail>{9}</EMail></Address></DeliveryAddress></Shipment><Order>{10}<OrderNumber>{11}</OrderNumber><PaymentStatus>2</PaymentStatus><ItemsTax>{12}</ItemsTax><Total>{13}</Total><ItemsTotal>{14}</ItemsTotal><BillingAddress><FirstName>{0}</FirstName><Company>{1}</Company><Address1>{2}</Address1><Address2>{3}</Address2><City>{4}</City><State>{5}</State><Country>{6}</Country><StateAsString>{5}</StateAsString><CountryAsString>{6}</CountryAsString><PostalCode>{7}</PostalCode><Phone>{8}</Phone><EMail>{9}</EMail></BillingAddress><ShippingAddress><FirstName>{0}</FirstName><Company>{1}</Company><Address1>{2}</Address1><Address2>{3}</Address2><City>{4}</City><State>{5}</State><Country>{6}</Country><StateAsString>{5}</StateAsString><CountryAsString>{6}</CountryAsString><PostalCode>{7}</PostalCode><Phone>{8}</Phone><EMail>{9}</EMail></ShippingAddress></Order></ShipTransaction></Request>";
        public const string itemPayloadTemplate = "<ShipmentOrderItem><Name>{0}</Name><Price>{1}</Price><Quantity>{2}</Quantity><Total>{3}</Total></ShipmentOrderItem>";
        public const string countriesList = "US,CA,AF,AX,AL,DZ,AS,AD,AO,AI,AG,AR,AM,AW,AU,AT,AZ,BS,BH,BD,BB,BY,BE,BZ,BJ,BM,BT,BO,XB,BA,BW,BR,VG,BN,BG,BF,BI,KH,CM,IC,CV,KY,CF,TD,JE,CL,CN,CX,CC,CO,KM,CG,CK,CR,CI,HR,CU,XC,AN,CY,CZ,DK,DJ,DM,DO,TL,TP,EC,EG,SV,GQ,ER,EE,SZ,ET,FK,FO,FJ,FI,FR,GF,PF,GA,GM,GE,DE,GH,GI,GR,GL,GD,GP,GU,GT,GG,GN,GW,GY,HT,HN,HK,HU,IS,IN,ID,IR,IQ,IE,IM,IL,IT,JM,JP,JO,KZ,KE,KI,KR,KV,KW,KG,LA,LV,LB,LS,LR,LY,LI,LT,LU,MO,MK,MG,MW,MY,MV,ML,MT,MH,MQ,MR,MU,YT,MX,FM,MD,MC,MN,ME,MS,MA,MZ,MM,NA,NR,NP,NL,NC,NZ,NI,NE,NG,NU,NF,KP,XS,NP2NO,OM,PK,PW,PS,PA,PG,PY,PE,PH,PN,PL,PT,PR,QA,RE,RO,RU,RW,BQ,LC,MP,WS,SM,ST,SA,SN,RS,SC,SL,SG,SK,SI,SB,SO,ZA,GS,SS,ES,LK,KN,MF,VC,XY,XE,SH,XM,PM,SD,SR,SJ,SE,CH,SY,TW,TJ,TZ,TW,TH,TG,TK,TO,TT,TN,TR,TM,TC,TV,VI,UG,UA,AE,GB,UY,UZ,VU,VA,VE,VN,WF,EH,YE,CD,ZM,ZW";
        public const string statesList = "AK,AL,AR,AZ,CA,CO,CT,DC,DE,FL,GA,HI,IA,ID,IL,IN,KS,KY,LA,MA,MD,ME,MI,MN,MO,MS,MT,NC,ND,NE,NH,NJ,NM,NV,NY,OH,OK,OR,PA,RI,SC,SD,TN,TX,UT,VA,VT,WA,WI,WV,WY,AS,CZ,FM,GU,MH,MP,PR,PW,VI,AA,AE,AP";
    }
}
