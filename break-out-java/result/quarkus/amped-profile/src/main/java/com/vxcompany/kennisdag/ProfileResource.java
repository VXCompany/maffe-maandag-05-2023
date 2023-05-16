package com.vxcompany.kennisdag;

import io.quarkus.security.Authenticated;
import io.quarkus.security.ForbiddenException;
import jakarta.enterprise.context.RequestScoped;
import jakarta.inject.Inject;
import jakarta.transaction.Transactional;
import jakarta.ws.rs.*;
import jakarta.ws.rs.core.Context;
import jakarta.ws.rs.core.Response;
import jakarta.ws.rs.core.UriInfo;
import org.eclipse.microprofile.jwt.Claim;
import org.jboss.resteasy.reactive.NoCache;
import io.quarkus.security.identity.SecurityIdentity;

import java.net.URI;

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
    public Response me() {

        if (!scope.contains(" read:profile ")) throw new ForbiddenException();

        String userId = securityIdentity.getPrincipal().getName();
        Profile profile = Profile.findByUserId(userId);

        if (profile == null) throw new NotFoundException("Unknown profile");

        return Response.ok(profile).build();
    }

    @GET
    @Path("/{nickName}")
    public Response user(String nickName) {
        if (!scope.contains(" read:profile ")) throw new ForbiddenException();

        Profile profile = Profile.findByNickName(nickName);

        if (profile == null) throw new NotFoundException("Unknown profile");

        return Response.ok(profile).build();
    }

    @Transactional
    @POST
    @Consumes("application/json")
    @Produces("application/json")
    @Path("/")
    public Response add(Profile profileToSave, @Context UriInfo uriInfo) {

        if (!scope.contains(" write:profile ")) throw new ForbiddenException();

        String userId = securityIdentity.getPrincipal().getName();
        Profile profile = new Profile();

        if (Profile.findByUserId(userId) == null) {
            profile.nickName = profileToSave.nickName;
            profile.bio = profileToSave.bio;
            profile.userId = userId;
            profile.persist();
        }

        URI uri = uriInfo.getAbsolutePathBuilder().path(profile.nickName).build();
        return Response.created(uri).entity(profile).build();
    }

    @Transactional
    @PUT
    @Consumes("application/json")
    @Path("/")
    public Response update(Profile profileToSave) {

        if (!scope.contains(" write:profile ")) throw new ForbiddenException();

        String userId = securityIdentity.getPrincipal().getName();
        Profile profile = Profile.findByUserId(userId);

        if (profile == null) throw new NotFoundException("Unknown profile");

        if (profileToSave.nickName != null) {
            profile.nickName = profileToSave.nickName;
        }

        if (profileToSave.bio != null) {
            profile.bio = profileToSave.bio;
        }

        return Response.ok().build();
    }
}
