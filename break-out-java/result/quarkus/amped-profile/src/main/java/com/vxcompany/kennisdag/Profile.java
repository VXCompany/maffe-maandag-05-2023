package com.vxcompany.kennisdag;

import io.quarkus.hibernate.orm.panache.PanacheEntityBase;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

import static jakarta.persistence.GenerationType.*;

@Entity
@Table(name = "profile")
public class Profile extends PanacheEntityBase {

    public String nickName;
    public String bio;

    @Id
    public String userId;

    public static Profile findByUserName(String userId) {
        return find("userId", userId).firstResult();
    }
}
