# Break Out Java

Ons uitgangspunt is de Profiles User Story. Deze kun je hier bekijken: [User Profiles](../../docs/backlog/2.%20User%20profiles.md).

We gaan er van uit, dat je de workshop in Quarkus 3 doet, maar het is natuurlijk ook aardig om het met Spring Boot 3 te proberen.

## De standaard: Quarkus 3

Zorg dat je minimaal Java 17 hebt geinstalleerd. Daarnaast maken we gebruik van de Quarkus CLI, dus die moet je ook installeren. 

JDK17: https://www.microsoft.com/openjdk of https://adoptium.net/

Quarkus: https://quarkus.io/guides/cli-tooling


## Het alternatief: Spring Boot 3.1

Zorg dat je minimaal Java 17 hebt geinstalleerd. Daarnaast maken we gebruik van de Quarkus CLI, dus die moet je ook installeren. 

JDK17: https://www.microsoft.com/openjdk of https://adoptium.net/



## Het project

In de ADR (Architectural Decision Record) staat wat informatie over het type API dat we kunnen gebruiken. Een ADR is eigenlijk de geschreven motivatie achter een (software)-architecturele keuze die je als team (zelfstandig of samen met een Software Architect) maakt. Het zorgt er voor, dat je op een later moment nog kunt "terughalen" onder welke omstandigheden (en met welke kennis) een beslissing genomen is. 

Bekijk de ADR voor de [AOT met Quarkus](../../docs/ADR/2.%20AOT%20met%20Quarkus.md).

```
quarkus create app com.vxcompany.kennisdag:amped-profile \
    --extension='oidc,resteasy-reactive-jackson' \
    --no-code
```

Tip: dit genereert een leeg Maven project. Mocht je wel sample code willen, dan moet je "no-code" weglaten.

## De User Story

Dit is een goed moment om samen even naar de User Story te kijken. Misschien zelfs even een korte refinement te plannen en een aanvalsplan te maken. 

[User Profiles](../../docs/backlog/2.%20User%20profiles.md)

Het persona wat hier bij hoort:

![](../../docs/personas/shelly.png)


## De techniek

Op basis van de refinement kunnen we aan de slag. Wij kozen voor de volgende componenten, maar uiteraard is die keuze helemaal aan jullie!

```
quarkus extension add 'quarkus-jdbc-h2'
quarkus extension add 'quarkus-hibernate-orm-panache'
```

* Een in-memory database. Kan ook met een repository o.i.d., maar in dit geval is een "echte" database wel handig.
* Database en Quarkus en Hibernate... dan is Panache wel een heel fijne optie https://quarkus.io/guides/hibernate-orm-panache
* Hier gebruiken we de Quarkus CLI. Kan natuurlijk ook met mvn.

## Aan de slag

Uit de User Story wordt duidelijk wat we moeten gaan bouwen. Er komt een punt, dat je de link met onze Identity Provider moet gaan leggen. Gebruik hier voor de gegevens die we apart zullen verstrekken.

Kijk of je de volgende Quarkus features tegenkomt en/of kunt (of zelfs moet) gebruiken:
* Hot Reload
* Developer UI
* AOT / Native Applications

## Tips

### Scope vs Role Based claims

Quarkus werkt uit de doos met Role Based claims. In deze workshop maken we liever gebruik van de (meer algemene) Resource Scope claims. Op zich geen probleem, maar de standaard annotaties voldoen dus niet. Onze oplossing was: Scope claim injecteren en in de methods controleren.

```java
    @Inject
    @Claim("scope")
    String scope;
```

```java
@GET
    @Path("/{nickName}")
    public Response user(String nickName) {
        if (!scope.contains(" read:profile ")) throw new ForbiddenException();

        Profile profile = Profile.findByNickName(nickName);

        if (profile == null) throw new NotFoundException("Unknown profile");

        return Response.ok(profile).build();
    }
```