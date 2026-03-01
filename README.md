# Bookovaní místa
Aplikace pro interní rezervování pracovního místa v kanceláři

# Instalace
TODO NuGet
SQLite

# Postup řešení, později # Využití aplikace
*vytvořím projekt. Přidávat názvy použitých NuGet

*začnu přidáním DB, použiju SQLite
vytvořím strukturu DB podle náčrtu v příloze

*připravím login na který využiju ASP.NET Core Identity

*jednotlivé stránky:
úvodní: 4 dlaždice
**akce zabookovat místo, přidat nová rezervace
**vypsat z DB rezervace.od, rezervace.do, misto.oznaceni, sekce.nazev pro aktuálního uživatele
**vypsat z DB misto.oznaceni, zamestnanci.jmeno kde rezervace.od < getDate() a rezervace.do > getDate() pro vybranou sekci, počítám s možností rezervace vůce dnů
**vypsat z DB misto.oznaceni, sekce.nazev, vytíženost, kde vytíženost jsou rezervované dny v poměru s výběrem, pouze pracovní dny, pro vybranou sekci + počet rezervovaných dnů

*Bookování místa
tlačítko na bookování > zobrazí celé patro > z obrázku se vybere místnost ze čtyř(devíti) možností > konkrétní místnost s možností rezervace
(beru jen naší stranu od hl. kuchyňky, devět pokud bych neodělovat dveřma, ale "zdmi")
v <svg> si nakreslím místnosti, udělat místo na jména pro Denní obsazenost a hover
zeleně volné místo, červeně obsazené, modře moje rezervace, světle modrá vybraná rezervace
tlačítko na potvrzení rezervace a výběr dne
u zabookovaných míst zobrazit rezervátora, hover na místo
možnost zabookovat jen jedno místo ve vybraný den
při potvrzení rezervace volat DB pro ověření aktuální dostupnosti

*Moje historie
nová stránka, tabulka pro hodnoty, výpis z DB, 100 záznamů, od nejnovějších

*Denní obsazenost
read-only grafika z Bookování místa
vždy zobrazovat jméno rezervátora
tlačítko na změnu data s defaultem na dnešku

*Vytíženost
tabulka podobně jako Moje historie
default jako celý aktuální měsíc
vypsat pouze místa s alespoň jednou rezervací
řadit podle počtu rezervovaných míst

# Dokumetace
DB struktura
Word doc

# DB model
https://dbdiagram.io/d
Table zamestnanci {
  id integer [primary key]
  jmeno varchar
  email varchar
}
Table sekce {
  id integer [primary key]
  nazev varchar
}
Table misto {
  id integer [primary key]
  sekce_id integer
  oznaceni varchar
}
Table rezervace {
  id integer [primary key]
  zamestnanec_id integer
  misto_id integer
  rezervovano_od date
  rezervovano_do date
}
Ref: "zamestnanci"."id" < "rezervace"."zamestnanec_id"
Ref: "misto"."id" < "rezervace"."misto_id"
Ref: "sekce"."id" < "misto"."sekce_id"
