# Spring is in the Air

## Setting
Als King Spring während einer seiner Reden vom Balkon stolpert und daraufhin in die Kanalisation fällt, findet er sich in einer unglücklichen Lage wieder. Der hinterhältige Lord Mortimer hat das Unglück des Königs ausgenutzt und sich kurzerhand selbst zum König gemacht, um das Königreich nach seinen eigennützigen Wünschen zu regieren. Das Königreich versinkt im Chaos und es liegt an King Spring, auf seinem Weg zurück nach oben in sein Schloss die zerbrochenen Stücke seiner Krone finden, um sein Recht auf den Thron zu beweisen.

## Gameplay
Spring is in the Air ist ein Platform-Climbing Spiel, angelehnt an *Jump King* und *Getting Over it with Bennett Foddy*. King Spring muss in verschiedenen Leveln auf Objekte und Plattformen und mehr springen, um nach oben zu gelangen. Die Level sind dadurch so strukturiert, dass der Spieler an vielen Stellen herunterfallen und seinen Forschritt verlieren kann. Durch das mehrfache Wiederholen der verschiedenen Sprünge bekommt der Spieler ein besseres Gefühl für die Sprungmechanik. Trotz des damit kommen Frusts soll er nach und nach die Sprungmechanik meistern, um erfolgreich die Level beenden zu können. Außerdem muss in jedem Gebiet ein Bruchstück der Krone gefunden werden, um weiter fortzuschreiten. Wenn King Spring mit einem Bewohner des Königreichs redet, wird er darauf hingewiesen, wo sich das Kronenstück befindet, falls er sie noch nicht eingesammelt hat.

## Characters
- King Spring, der König von Kingdom Spring
- Das Volk von Kingdom Spring, die unterstützenden Untertanen des Königs
- Der hilfsbereite Einwohner, am Einstieg in die Kanalisation
- Der letzte verbleibende Postbeamte, im Paketlager
- Mortimer, ein machthungriger Lord an King Springs Hof

## Spielablauf
- Einführung in Geschichte durch Introsequenz mit Bildern und Text
- kurze Einführung in Steuerung zum Spielstart
- in jedem Gebiet gibt es ein Kronenstück zu finden und am Ende einen Story-Dialog, in dem man mehr über die Umstände der Welt erfährt
- Gameloop: Spieler betritt neues Gebiet &#8594; arbeitet sich hoch und sammelt Kronstück &#8594; kann Gebiet abschließen &#8594; Spieler betritt neues Gebiet
- Endsequenz mit Bildern und Text zum Ende der Geschichte

## Movement- und Sprungmechanik
Der Spielercharakter ist komplett physikbasiert und kann durch das Ausüben von Kräften gesteuert werden. Sämtliche Interaktionen sowie Animationen werden dabei von Unity's Physikengine gehandhabt. 
Der Charakter besteht aus einer Kette von fünf unsichtbaren Würfeln (im Folgenden Glieder genannt), welche durch Spring Joints verbunden sind.

Wird er nicht vom Spieler bewegt, hat das unterste Glied eine Masse von 1,5 und wird logischerweise von der Erdanziehungskraft nach unten beschleunigt, alle anderen Glieder haben eine Masse von je 0,1 und werden mit derselben Erdbeschleunigung nach oben gezogen, wodurch es scheint als stünde King Spring aufrecht.
[Screenshots hier einfügen]
[Walking GIF einfügen]
Wird der Spielercharakter nach links oder rechts bewegt, wird alle 0,93 Sekunden die Coroutine „Walk“ gestartet. Diese bewegt zuerst das oberste Glied von King Spring durch Überschreiben seiner Geschwindigkeit in die gewünschte Richtung und tauscht kurze Zeit später die physikalischen Eigenschaften des obersten Gliedes mit dem 
Bei jedem Sprung überschlägt sich King Spring einmal selbst, genau wie beim Laufen auch. Vor/nach dem Überschlag
TODO Erdbeschleunigung definieren


## Quellen
Soundeffekte:
- Crown Pickup Sound Effect: [Happy Award Achievement](https://www.storyblocks.com/audio/stock/happy-award-achievement-hby-qcpmfplk8p0xm3q.html) von Storyblocks
- Walking Sound Effect 1: [Creaks](https://freesound.org/people/damsur/sounds/443244/) von damsur auf freesound
- Walking Sound Effect 2: [Creaks](https://freesound.org/people/damsur/sounds/443237/) von damsur auf freesound
- Pipe Collision Sound Effect: [Light_Steel_Pipe_On_Concrete_01.wav](https://freesound.org/people/dheming/sounds/177783/) aus dem „Metal“ Sample Pack von dheming auf Freesound
- Wood Collision Sound Effect: [Footsteps Running On Wooden Floor Fast Pace.wav](https://freesound.org/people/ralph.whitehead/sounds/565713/) aus dem „Footsteps Foley“ Sample Pack von ralph.whitehead auf freesound

