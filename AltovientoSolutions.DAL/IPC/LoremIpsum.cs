using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AltovientoSolutions.DAL.IPC.Model;
using AltovientoSolutions.Common.Util;

namespace AltovientoSolutions.DAL.IPC
{
    public class LoremIpsum
    {
        private static Random rnd = new Random(DateTime.Now.Millisecond);

        private static string[] wordBankEN = "Authoritatively optimize bricks-and-clicks schemas with backward-compatible technologies Competently engage impactful communities for unique solutions Proactively scale quality supply chains after turnkey portals Compellingly monetize seamless mindshare whereas an expanded array of products Rapidiously leverage other's user friendly partnerships through extensible outsourcing Objectively innovate pandemic methods of empowerment before diverse e-services Energistically aggregate strategic potentialities without standardized ideas Phosfluorescently restore an expanded array of quality vectors for next-generation testing procedures Phosfluorescently coordinate resource functionalities without enabled channels Professionally supply distributed results whereas intermandated total linkage Phosfluorescently provide access to integrated content without accurate web-readiness Conveniently evolve user-centric infrastructures after distinctive bandwidth Seamlessly e-enable real-time customer service via fully".Split(' ');
        private static string[] wordBankFR = "UN VIL FOU CHAS LOI RANCUNIER BOUTENT REDEVRA RETAPISSERONS VALUS GRIVES GRÉ INHIBENT LOT CAS MODÈLES REPRENEZ NU CT NO AUBE PICS OFFICIEUSES BUE NAVALE SURS VULGARISASSES AUTRE KILOS ENCROÛTÉS FUMA AI FEUX DIX SALERAI COÛTA BILE AXAI TRUQUANT TRIONS DAMNONS DÉCROCHÂTES CHANTRE EUX LUE DATE RAS SHOOT AGIOS CECI GÂTERAIS AIES PÉNALISER IRRIGUONS BRUNISSIONS CE OS RADAI CUIVRÂMES INDIGNIEZ EXIL OBSERVA MEURT ADONNASSE SOT LOADERS LA CR FILMER AV SUPPLÉAIS REFIRENT BERNÂTES RÉVOLUTIONNEZ CARTE DATES BÉE DU SCIONS JUDO DM PRIEZ DUT LENTE AMI MAT QUINTE EAU INDIANISTES MUEZ VAUTRÂMES TRANSVIDER COUPLE ATTERRÉ CONCAVE COUPLA DUE TURF CE VANS ÉPI MURA CHAT GRUE TOND NEIGE DUT BAR CLABOTER MURÉE MOL DÉTAILS DENT MONOLOGUEZ PUAIT ÉTUVAIENT COUVRIRIONS CH GLISSASSENT PAGINERONS COULERAIT COMBATTRA PAGNES RAS TRACTEREZ GUI SI MARINÉS PERLÉE MUFLE MI PÈLE GÂTE GEL AIDA NEUTRE JOUE ENGORGÉE IL AI TUERIES ENNUYA PROSTATE SUCRAS DÉGAZONNERAS AC LBS DÉCALE ISOLANTS MA DÉDIAIS ION AV RELATIVISER PARUS ABUS SERRE".Split(' ');
        private static string[] wordBankDE = "ER EIGNE UM MÜHT MATT ICH WAGE SÄGE IM TROMPETETET BIN REGLEMENTIERENDE SCHEUERE REICHTE HER NUN ARG PRO AB RESULTIERT FRISIERTE VORM HOHL SO HINGEGEN SEGNE AN RAR BEHAUPTEND VOR DIE DRIN WACHSAM ZIELEN DU GÄHNE HORT FROTTIERTE FÜGT WO ANTRAF ESSE DU HAARGENAUEN BIN HIE ZERKAUENDE SPARTE WENIG ABFÄRBEN FAULER SOWIE GETAUFT LÄHMT SPÄRLICHSTEN BADE RECKTEN GESTIKULIERTET DRIN POLNISCHE GEIL JEMAND EDLES BASTELN WEIBLICH UNFEHLBAR HAKE SHOW TRAGBAR BOG ERTRINKE WILLIG FRAGLOSER BIST SANITÄR IMPFT DRESSIERTE VON DURCHGESCHLAGENEN OB UNS AB GELB SEH FÜNDIG HEIL DARUM WEITERGEBRACHTER WETTENDER FORMVOLLENDETEM GOTISCHE GEHT SÄEN ES RUND JA GEKNETET UMSPULEN SPEICHERST UNARTIGSTER PROST ÖDE REGT UNS BOT MALT HALF BANG ÖLTET NENNT UNGERADER ÜBERHÖRT GUT FIX DA SHOW OFT SEGEL WO TOBE BY SEID UM MAN PRO JA VERHERRLICHE ALL GESATTELT ZUR ZAUBERND BELEGENE RAGTE BY SCHIEßEN PASSIONIERTERER FLUCHE MISSEST WENDETE STICKT OFT MUß BLAU PUNKTÜLL IM LOG PHILOSOPHISCHE BEQUEMERES VOR TRABE RIEF FLIRTENDE BLAUES GRASEN".Split(' ');
        private static string[] wordBankES = "CONSUMAMOS CUBA ECO PRUEBA IZADA SITO GÁRGARAS LENIR FAZ TRAJÍN OCHO MU SALITA ME ME BOU Y BUITRERAS TIJERETAZOS FAS TEJE ROMEA DESEAD MUDAR SIGNIFICAD FÍE Y CHIMPANCÉS FUSTIGACIÓN DESALIENTAN LAR CERDEAR ROE DESHONRADOS SE SOBO UVA CONVERSABA LA MIÓ ESTRENO DRIL HOCE MUDABLES EX DIO CRASAS DESOYEN NAUFRAGÓ PLEITEADA CLAN AZORAR FE RADA UNÍS AS TEN BUSES AHONDÉIS MEDITO NI CACHETEA DESPEJARLAS AH TIMOS FÍA FEZ MAMEN LORO APAÑAR VISA KA ORE AMAD CHAFABAS ÁLEF CADERA A CEGARLO RECORRERÁ DEMERITORIAS PLEGASTE AGÁ CONDUCIRLA SUS I ONDEADO USE ÁRIDA MOLDARLOS SEA A PUDRE TUS RELAMÍA U TRASEGARLES ENSOPADA IR TAMIL BAJEMOS PRELATIVO LARGUES OH RIZÓN UNAS ENVAINARÁ TEJA U GUINEA ARREPINTIÉRAMOS SOCIÓLOGAS APILE SOY RITO LAR COMAS USOS HURTEN CATEO NOXA BAJABA DES ESTARZAN DIMITIERAS TOMO BOSTECEMOS YA AH RESISTIESES Y TI REALIZÁNDOLA TRAS MEJORÍA A RESOBRINA CONFIGURADOS JUBILOSAS PUROS LIS DOBLES CHE GANA E FEZ YE LEMNÁCEA GE IDO GOBERNASEN APIO CON APRECIAD VE JAZZ MANÍ OSO RINDE VOTE ME DRAMATIZARÉ".Split(' ');
        private static string[] wordBankPT = "MORREM SUTÁMO GRUDAR ASCETIZÁ ARREVESEIS TA DEFENDAMO TALIÕES AR DESMILITARIZAR VEM SWING AL NAPA ENTESOIRE SONDARE SÓ ANURO GEEI ZULOU FREI PÁS JITOS CANO ACARREJANDO NO ERRA PERO RAIVÓ IR BUS SIEI ADUNADA DO JAUS IRAI MENÇÃO FECHAS PIS TRISSASSEM MORDOMO DEVI CO ABAFAREMOS CICIA E BOOM VERE GORGOLÁVAMOS ÁS RESSEGAS DESESPINHARA PEDINCHARE ESCORIEM O UPE EMBOCARAS RE LOBO CETINAS OCA PÉRAS E SACAI PAZ SUBSTANTIFICANDO APRECIAMO ALCATRUZASSES NO ATRAVÉS BIDÉS DÊ AI MA ABO BAILE ESPIGUEMOS UDU ÓS CAJARANAS MAGO IN O RIPÁ E MARULHAM REDREM AGE ZUCOU LHO TAPEÇÁREI BEDEL DESCARREGASSE BOCEJOU DOAMO NEGO ENE GEMADO E LAPIÁS KV DESCULTIVES DIR KW ENRIEIS LA COSA CHILREADAS VITIMEMOS IÇO FAR ETERNANDO EXPIARÁS MENORÍTICOS DELE ATACO TRUFEI HEI SABOTAVA SR JUDIAM PLAINETE ÉS SEGADA TUJAMOS RENTA DE MIRI ESCOVILHEIRAS RUELA VIZINHANÇAS BRAÇOLA KV POUSA AIA ÁS PNEUMOGÁSTRICO ELE VISOS MELOU LO ORASSEM HÁ DÊ AFÁ TRAFEGUES ALOR EMPUBESCE UIVAM TV AÇORE ENGRANZAR MIRA A SUICIDÁVEIS GIBAS CONO O TUTELES VIBRÁ".Split(' ');
        private static string[] wordBankDU = "JEU CENSUREREN GEWIT DOEN VOL PO MAL KALEBAS STOERDER ADEL GERAAK MAGERTE EI WALS GERESULTEERDEN LAKT KEITJE TELLER NAVROEG MONSTRUEUS SPRINGALEN VERZIEND BLOEDSCHENDIG PLAST KOUSENVOETEN DONKERBRUINER IN UT PIEL JE HU KRUK LIJVEN EB BUIG KIP EI REN ZOG MISDUID DOWN VAALBLAUW OVEREIND FRATS VRILLE ARM KAAS SPIEGELING ARM UITGESCHUTTE LAS ONZEDIGER DE TREKEN DOVE OEN METALLURG TRIESTHEID MEESTERES DITJE INDIENDE BUIL PIJ RADELOOST PUNK GALMT HEK PAS ALS LENS GEELDE BEZIN AANKLAAGDEN ONDER MICA ZADEN ROTTING GEPOOKT JU UITLEKTE AARDT RESISTENTST FORMULIEREN GRAS UITBUNDIG SLONS MALTA WC MI BAJONET PEE BETRAAND MUON ALBINO BROEDEREN NOEST SOLDEN MONOMAAN ME REG MIJT TIP AF MEI AS SCHRAAPSEL NASALEERT ITERATIEMODEL FOUT IN PROMOOT GEKARND NAR AANSPRAKELIJKHEIDSPOLIS AT DEFILEERDE RATA TOEN VOD HOI KLOET DOK BITS SET ENG ACCEPTEN ZWAARMOEDIGSTE LOUTERAAR KERFBIJL RENTES PIEKUUR INWIPT VOORBEHANDELING NAVEN AFWENNING EER SLOK NATST PUIM TERNE IE POETST VERLEREN LUIM POES LAG BEROEPSWERKZAAMHEID SIKJE".Split(' ');
        private static string[] wordBankZN = "圩芰 摮 虰豖阹 腷腯葹 汫汭沎, 馦騧騜 躨钀钁 圪妀 魡, 瀁瀎瀊 庌弝彶 瑽 竀篴 嬏嶟樀 揯揳揓 蠬襱覾 嵷 鋄銶, 橀 錖霒馞 燲獯璯 蛣袹 鑆驈 鳱儇嘽 鋱鋟鋈 毊灚襳 潣 蔰 絼綒 蚔趵郚 墥墡嬇 鞈頨頧, 榳榓 雗雘雝 蹸蹪鏂 禠 榃痯痻 轗鐔飂 輠 絼綒, 蝺 轖轕 餤駰鬳 邆錉霋 炾笀耔 嬦憼 滆 耜僇鄗 驧鬤鸕 沀皯竻 銙 驧鬤 烗猀珖 梜淊淭, 橀槶澉 蛚袲褁 溗煂 魵 繗繓 顤鰩鷎 貵趀跅 墐, 驐鷑 捘栒毤 梴棆棎 鷖鼳鼲 撱 釸釪傛 溗煂獂 漈禊禓 揳揓 葮 莦莚 蜙 輲輹輴 涬淠淉 毊灚襳, 貄趎跰 驨訑紱 泏狔狑 訑紱 銪 溾滘 氉燡磼 烳牼翐 輘 雗雘雝 峬峿峹 剆坲 腠, 跣 鬐鶤 鑕鬞鬠 蔪蓩蔮, 縢羱聬 鎈巂鞪 咶垞姳 圔塥 槶 緦膣膗 鋑鋡髬 曏樴橉 皾籈 跾, 齥廲 簥翷臒 磃箹糈 鳼 歅 罞耖茭 瀤瀪璷 灊灅甗 纑臞, 樀樛 歅 蛃袚觙 螏螉褩 訬軗郲, 鯦鯢鯡 忷扴汥 磑 冹妎 杍肜 磏 繠繗繓 鼥儴壛, 蝯蝺 馺 壾嵷幓 齴讘麡 蝺 胾臷菨 觓倎哤 貹貵, 緁 趀跅 枅杺枙 鐩闤鞿 蝑蝞蝢 壾 焟硱 謺貙蹖 嫬廙彯 鐩闤鞿 鄻鎟霣 逜郰傃 澂 黈龠 殽毰毲 隒雸頍 樧槧樈 靾鞂 漀, 詵貄 嗢嗂塝 鶭黮齥 樛槷殦 跬 摲 醳鏻 輑鄟銆 熩熝犚 褌 淊淭 歅毼毹 駺駹鮚 芘芛芤, 鍗鍷 忁曨曣 鑕鬞鬠 脬舑莕 摮, 稢綌 緳廞徲 溛滁溒 幓 禖穊稯 爂犤繵 炾笀耔 鸃鼞 榯, 毹 蘹蠮 箛箙舕 寁崏庲 幎愶戣".Split(' ');
        private static string[] wordBankRU = "На времени собственный кто, код об меньше работается. Нее ушёл размером но. Уже внешних собственный ну, ты были адрес очень лет. Ну оно мирам взлета языках, не какие сохранение код Ваших кажется от не, делала создаете лет от, нью какие налево общеизвестно вы. Те вернёмся университет эти, хочу новые убегай от мог. Как ну когда забудем определения, они по никто может примерно. Сажусь разные потому до кто, за письмо полностью для. Буду плиты без не, другие полугода его он Плиты лучшему программы те эти. До нас английски связанном, быстро стоить йельский во чем, никто соответствующим том но. Он глупые порулил вернуться три, платформ творческую биг не, эти никто только сэкономленного до. Миф деле работу успехов то. Код делала предназначенная вы, уже мы минуту мнение постигнет. Ещё минуту знания отказаться бы, любых трудовой кто на Как могу плиту ставить те, те лет любых знаем систему. По вопрос раздутых чем, нее по размером проектировщик. Лет цели принять но, вот одного качества ты, вы этой запустить тем".Split(' ');
        private static string[] wordBankHB = "בחירות קישורים אנא ב אם בידור אנגלית לאחרונה צ'ט המלצת העברית אם קרן של כלשהו עקרונות סטטיסטיקה שכל את תרומה מיותר לויקיפדים אחד היא גם מיוחדים למאמרים המקושרים אל קהילה שיתופית מדויקים תנך בה ניהול טכניים היא דת כדי כימיה מרצועת שינויים בקר שמות טיפול אל בשפות שימושיים את בדף אנא דת למתחילים טכנולוגיה גם סדר שנורו בכפוף משפטית אינטרנט מדריכים גיאוגרפיה ארץ בה בלשנות המקושרים גם לוח דת כתב שונה משחקים שינויים דת ארץ ערכים וכמקובל מוגש למנוע המחשב מדע של מה איטליה החופשית האטמוספירה צ'ט רבה אל אינו תורת תבניות שתי גם קהילה ספינות תחבורה את כדי פיסול המלחמה אם זקוק שאלות וקשקש קרן אם רביעי עקרונות מתן טיפול רומנית לחשבון על סדר של קולנוע מבוקשים חפש אתה אל יוני והנדסה קצרמרים יידיש בישול אם אחד שער כדור באגים למחיקה גם צעד כימיה משופרות או זכויות ממונרכיה או בקר אינו רשימות שמו גם חפש המזנון ברוכים מונחונים את תנ הרוח באגים אל ב הגרפים האטמוספירה אחר שער בשפה היסטוריה אל גם עסקים ספרדית מדריכים זכר כדי בה אחרים בלשנות אספרנטו שנורו עסקים והנדסה צעד של מחליטה בעברית גם פולנית לויקיפדיה שער הנדסת המלחמה ארכיאולוגיה או צעד אחד אל להפוך לערך אדריכלות וספציפיים זאת דת שדרות".Split(' ');
        private static string[] wordBankAR = "مما في ماشاء المتطرّف خصوصا لتقليعة الإمبراطورية كلا هو كل شبح وأسرت وكسبت أنجلو-فرنسية صفحة أواخر تلك أم الآخر طائرات كل ضرب غير دارت ألماني التبرعات بـ حدى القوات تحرّكت بل حيث كل غرّة، لفرنسا والروسية بـ دون خلاف لقهر المتاخمة و حدة أفاق الحكومة أوزار ستالينجراد في الى كل شيء بيرل الثانية ستالينجراد عل الا الأمور الموسوعة ثم انه عقبت العدّ الإمبراطورية تحت فسقط أحدث السيطرة لم أمدها الأمور الرايخ جعل هو ببعض بولندا، الأوروبية ثم جُل الفترة الإحتفاظ في ولم إيو كل بسبب إستمات الجنرال عرض هو عجّل والتي قد قصف أهّل أراضي قد للجزر إستمات الساحل مدن و حشد ألمّ إستمات أجزاء الفترة ولكسمبورغ وقد مع الخاسرة الدّفاع للألمان في حين وقبل بالعمل واستسلم عرض عل غزوه قررت العسكرية حدة ما عل عرض تجهيز البرية وصافرات و ولم الحاملات الأيديولوجية، من وعُرفت ألماني الغازية ومن الشمل البلطيق منهمكتين ثم بها بل أما كارثة المنتصر لبولندا موالية المناوشات أم حدى ثم وحتى للصين التحالف تعد تونس جيوب ذات عن شيء حقول الهادي عن عن حربية وانهاء المعارك إيو وجزر الصيني الأوروبيّون تلك قد أن بين وحتى لغزو جوزيف تحت عرفها ماشاء العالم ما بدفع الأخذ العمليات و أخر هو هزيمة الفرنسية ذلك دنو بتطويق حاملات والألمانية، و ربع".Split(' ');

        private static string[] supportedLanguages = "en,fr,de,es,pt,du,zn,ru,hb,ar".Split(',');

        //public Catalog GetCatalog(string ID, string LanguageCode)
        //{
        //    string[] languages = new string[] { LanguageCode };
        //    Dictionary<String, Catalog> catalog = GenerateSampleCatalog(ID, languages);


        //    return catalog[LanguageCode];
        //}

        public static Dictionary<String, Catalog> GenerateSampleCatalog(string ID, string[] Languages, out Dictionary<string, byte[]> Illustrations)
        {
            int numberOfChapters = 4 + rnd.Next(15);
            int maxNumberOfPages = 5 + rnd.Next(40);
            int maxNumberOfEntries = 5 + rnd.Next(50);
            return GenerateSampleCatalog(ID, numberOfChapters, maxNumberOfPages, maxNumberOfEntries, Languages, out Illustrations);
        }

        public static Dictionary<String, Catalog> GenerateSampleCatalog(string ID, int NoOfChapters, int MaxNoOfPages, int NoOfEntries, string[] Languages, out Dictionary<string, byte[]> Illustrations)
        {
            if (String.IsNullOrWhiteSpace(ID))
            {
                throw new ArgumentException("ID cannot be null or empty");
            }
            
            // Generates a dummy catalog in English.
            Catalog catalog = GenerateSingleCatalog(ID, NoOfChapters, MaxNoOfPages, NoOfEntries, "en", out Illustrations);

            Dictionary<String, Catalog> publishedCatalogs = new Dictionary<string, Catalog>();
            publishedCatalogs.Add("en", catalog);
            
            
            String[] requestedLanguages = (from l in Languages
                                               where supportedLanguages.Contains(l)
                                               select l).ToArray();
            
            foreach (string lang in requestedLanguages)
            {
                if (lang == "en")
                    continue;

                Catalog translatedCatalog = TranslateCatalog(lang, catalog);
                publishedCatalogs.Add(lang, translatedCatalog);
            }

//            foreach (KeyValuePair<string, byte[]> image in images)
//            {
//#warning Need to define how to handle spaces and where to store the configuration.

//                IPCMediatorMongoDB ipcMediator = new IPCMediatorMongoDB("ipcviewer_demo");
//                ipcMediator.SaveIllustration(image.Value, "Space_00001", image.Key, image.Key);

//            }

            return publishedCatalogs;
        }

        private static Catalog GenerateSingleCatalog(string ID, int NoOfChapters, int MaxNoOfPages, int NoOfEntries, string langCode, out Dictionary<string, byte[]> images)
        {
            images = new Dictionary<string, byte[]>();
            Catalog catalog = new Catalog();

            catalog.ID = ID;
            catalog.Title = GetRandomWords(4, langCode);
            catalog.LanguageCode = langCode;

            for (int i = 0; i < NoOfChapters; i++)
            {
                Chapter chapter = new Chapter();
                chapter.ID = Guid.NewGuid().ToString();
                chapter.Title = GetRandomWords(3, langCode);

                int numberOfPages = 5 + rnd.Next(MaxNoOfPages);
                for (int j = 0; j < numberOfPages; j++)
                {
                    Page pg = new Page() { ID = Guid.NewGuid().ToString() };
                    pg.Title = GetRandomWords(5, langCode);

                    int numberOfEntries = 3 + rnd.Next(NoOfEntries);
                    for (int k = 1; k < numberOfEntries + 1; k++)
                    {
                        Entry entry = new Entry() { ID = Guid.NewGuid().ToString() };
                        entry.Sequence = k;
                        entry.Position = k.ToString();
                        entry.MaterialNumber = rnd.Next(15) < 1 ? String.Empty : Guid.NewGuid().ToString().Split('-')[0];
                        entry.Quantity = Convert.ToString(rnd.Next(2) < 1 ? 1 : rnd.Next(6));
                        entry.UOM = "UNIT";
                        entry.Description = GetRandomWords(6, langCode);
                        entry.Comment = String.Empty;
                        entry.Comment = Convert.ToString(rnd.Next(3) < 1 ? String.Empty : GetRandomWords(6, langCode));
                        entry.Comment += Convert.ToString(rnd.Next(5) < 1 ? String.Empty : "<br/>" + GetRandomWords(6, langCode));
                        entry.Comment += Convert.ToString(rnd.Next(8) < 1 ? String.Empty : "<br/>" + GetRandomWords(6, langCode));
                        entry.Comment += Convert.ToString(rnd.Next(10) < 1 ? String.Empty : "<br/>" + GetRandomWords(6, langCode));
                        pg.Entry.Add(entry);

                    }

                    List<Callout> callouts = new List<Callout>();
                    Bitmap img = IllustrationIpsum.GenerateRandomImage(1600, 1200, numberOfEntries, true, out callouts);

                    pg.Callout = callouts;

                    MemoryStream ms = new MemoryStream(61440);
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                    byte[] imgBuffer = ms.ToArray();
                    pg.IllustrationID = Password.EncodeBinary(imgBuffer);

                    if (!images.ContainsKey(pg.IllustrationID))
                        images.Add(pg.IllustrationID, imgBuffer);

                    try
                    {
                        chapter.Page.Add(pg);
                    }
                    catch (Exception e)
                    {
                        chapter.Page.Add(pg);
                    }
                }

                    catalog.Chapter.Add(chapter);
            }


            return catalog;
        }

        private static Catalog TranslateCatalog(string langCode, Catalog originalCatalog)
        {
            Catalog catalog = new Catalog();

            catalog.ID = originalCatalog.ID;
            catalog.Title = GetRandomWords(4, langCode);
            catalog.LanguageCode = langCode;


            foreach (Chapter originalChapter in originalCatalog.Chapter)
            {
                Chapter chapter = new Chapter();
                chapter.ID = originalChapter.ID;
                chapter.Title = GetRandomWords(3, langCode);

                foreach (Page originalPage in originalChapter.Page)
                {
                    Page pg = new Page() { ID = originalPage.ID };
                    pg.Title = GetRandomWords(5, langCode);
                    pg.IllustrationID = originalPage.IllustrationID;
                    pg.Callout.AddRange(originalPage.Callout);

                    foreach (Entry originalEntry in originalPage.Entry)
                    {
                        Entry entry = new Entry() { ID = originalEntry.ID };
                        entry.Sequence = originalEntry.Sequence;
                        entry.Position = originalEntry.Position;
                        entry.MaterialNumber = originalEntry.MaterialNumber;
                        entry.Quantity = originalEntry.Quantity;
                        entry.UOM = "UNIT";
                        entry.Description = GetRandomWords(6, langCode);
                        entry.Comment = String.Empty;
                        entry.Comment = Convert.ToString(rnd.Next(3) < 1 ? String.Empty : GetRandomWords(6, langCode));
                        entry.Comment += Convert.ToString(rnd.Next(5) < 1 ? String.Empty : "<br/>" + GetRandomWords(6, langCode));
                        entry.Comment += Convert.ToString(rnd.Next(8) < 1 ? String.Empty : "<br/>" + GetRandomWords(6, langCode));
                        entry.Comment += Convert.ToString(rnd.Next(10) < 1 ? String.Empty : "<br/>" + GetRandomWords(6, langCode));
                        pg.Entry.Add(entry);
                    }

                    chapter.Page.Add(pg);

                }
                catalog.Chapter.Add(chapter);
            }



            return catalog;
        }

        private static string GetRandomWords(int maxNumberOfWords, string langCode)
        {
            StringBuilder sb = new StringBuilder(100);

            string[] wordBank;
            switch (langCode)
            {
                case "en":
                    wordBank = wordBankEN;
                    break;
                case "fr":
                    wordBank = wordBankFR;
                    break;
                case "de":
                    wordBank = wordBankDE;
                    break;
                case "es":
                    wordBank = wordBankES;
                    break;
                case "pt":
                    wordBank = wordBankPT;
                    break;
                case "du":
                    wordBank = wordBankDU;
                    break;
                case "ar":
                    wordBank = wordBankAR;
                    break;
                case "hb":
                    wordBank = wordBankHB;
                    break;
                case "ru":
                    wordBank = wordBankRU;
                    break;
                case "zn":
                    wordBank = wordBankZN;
                    break;
                default:
                    goto case "en";
            }


            int numberOfWords = 1 + rnd.Next(maxNumberOfWords);


            for (int i = 0; i < numberOfWords; i++)
            {
                sb.Append(wordBank.GetValue(rnd.Next(wordBank.Length)) + " ");
            }

            return sb.ToString().ToUpper().Trim();
        }

    }
}
