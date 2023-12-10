import { Link } from "gatsby";
import * as React from "react";
import logo from "../../images/logo.svg";

const Navbar = (): JSX.Element => {
  const [navbarActive, setNavbarActive] = React.useState(false);
  const toggleHamburger = () => {
    setNavbarActive(!navbarActive);
  };
  return (
    <nav
      className="navbar is-transparent"
      role="navigation"
      aria-label="main-navigation"
    >
      <div className="container">
        <div className="navbar-brand">
          <Link to="/" className="navbar-item" title="Logo">
            <img
              src={logo}
              alt="Spring"
              style={{ width: "180px", maxHeight: "48px" }}
            />
          </Link>
          {/* Hamburger menu */}
          <div
            className={`navbar-burger burger ${
              navbarActive ? "is-active" : ""
            }`}
            data-target="navMenu"
            role="menuitem"
            tabIndex={0}
            onKeyPress={() => toggleHamburger()}
            onClick={() => toggleHamburger()}
          >
            <span />
            <span />
            <span />
          </div>
        </div>
        <div
          id="navMenu"
          className={`navbar-menu ${navbarActive ? "is-active" : ""}`}
        >
          <div className="navbar-end has-text-centered">
            <div className="navbar-item has-dropdown is-hoverable">
              <Link className="navbar-link" to="/overview">
                Learn
              </Link>
              <div className="navbar-dropdown is-boxed">
                <Link className="navbar-item" to="/overview">
                  Overview
                </Link>
                <Link className="navbar-item" to="/modules">
                  Modules
                </Link>
                <Link className="navbar-item" to="/roadmap">
                  Roadmap
                </Link>
                <Link className="navbar-item" to="/documentation">
                  Documentation
                </Link>
                <Link className="navbar-item" to="/examples">
                  Examples/Tutorials
                </Link>
              </div>
            </div>

            <Link className="navbar-item" to="/download">
              Download
            </Link>
            <Link className="navbar-item" to="/news">
              News
            </Link>

            {/* <Link className="navbar-item" to="/training">
              Training
            </Link>
            <Link className="navbar-item" to="/support">
              Support
            </Link> */}
            <div className="navbar-item has-dropdown is-hoverable">
              <span className="navbar-link">Community</span>
              <div className="navbar-dropdown is-boxed is-right">
                <a
                  className="navbar-item"
                  target={`_blank`}
                  href="http://forum.springframework.net/"
                >
                  Forum
                </a>
                <a
                  className="navbar-item"
                  target={`_blank`}
                  href="https://jira.springframework.org/secure/BrowseProject.jspa?id=10020"
                >
                  JIRA Issue Tracking
                </a>
                <a
                  className="navbar-item"
                  target={`_blank`}
                  href="https://fisheye.springframework.org/browse/spring-net"
                >
                  FishEye Repository Browser
                </a>
                <a
                  className="navbar-item"
                  target={`_blank`}
                  href="http://build.springframework.org/browse/SPRNET"
                >
                  Continuous Integration
                </a>
                <a
                  className="navbar-item"
                  target={`_blank`}
                  href="https://github.com/springsource/spring-net/"
                >
                  Source Repository (GitHub)
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
