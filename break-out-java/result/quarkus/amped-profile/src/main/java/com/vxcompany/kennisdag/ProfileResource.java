package com.vxcompany.kennisdag;

import io.quarkus.security.Authenticated;
import jakarta.inject.Inject;
import jakarta.ws.rs.GET;
import jakarta.ws.rs.NotFoundException;
import jakarta.ws.rs.Path;

import org.jboss.resteasy.reactive.NoCache;
import io.quarkus.security.identity.SecurityIdentity;

@Path("/profile")
public class ProfileResource {
    @Inject
    SecurityIdentity securityIdentity;

    @GET
    @Path("/")
    @Authenticated
    @NoCache
    public Profile me() {

        String userId = securityIdentity.getPrincipal().getName();
        Profile profile = Profile.findByUserName(userId);

        if (profile == null) throw new NotFoundException("Unknown profile");

        return profile;
    }
}
