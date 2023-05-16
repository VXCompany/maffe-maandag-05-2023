package com.vxcompany.kennisdag;

import io.quarkus.security.Authenticated;
import io.quarkus.security.ForbiddenException;
import jakarta.enterprise.context.RequestScoped;
import jakarta.inject.Inject;
import jakarta.transaction.Transactional;
import jakarta.ws.rs.*;
import org.eclipse.microprofile.jwt.Claim;
import org.jboss.resteasy.reactive.NoCache;
import io.quarkus.security.identity.SecurityIdentity;

@Path("/profile")
@RequestScoped
@Authenticated
@NoCache
public class ProfileResource {
    @Inject
    SecurityIdentity securityIdentity;

    @Inject
    @Claim("scope")
    String scope;

    @GET
    @Path("/")
    public Profile me() {

        if (!scope.contains(" read:profile ")) throw new ForbiddenException();

        String userId = securityIdentity.getPrincipal().getName();
        Profile profile = Profile.findByUserId(userId);

        if (profile == null) throw new NotFoundException("Unknown profile");

        return profile;
    }

    @GET
    @Path("/{nickName}")
    public Profile user(String nickName) {
        if (!scope.contains(" read:profile ")) throw new ForbiddenException();

        Profile profile = Profile.findByNickName(nickName);

        if (profile == null) throw new NotFoundException("Unknown profile");

        return profile;
    }

    @Transactional
    @POST
    @Consumes("application/json")
    @Produces("application/json")
    @Path("/")
    public Profile add(Profile profileToSave) {

        if (!scope.contains(" write:profile ")) throw new ForbiddenException();

        String userId = securityIdentity.getPrincipal().getName();

        Profile profile = new Profile();

        if (Profile.findByUserId(userId) == null) {
            profile.nickName = profileToSave.nickName;
            profile.bio = profileToSave.bio;
            profile.userId = userId;
            profile.persist();
        }

       return profile;
    }
}
