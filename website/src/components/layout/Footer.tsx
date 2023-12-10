import { Link } from "gatsby";
import * as React from "react";
import logo from "../../images/logo.svg";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faJira, faGithub } from "@fortawesome/free-brands-svg-icons";

import { faMessage } from "@fortawesome/free-solid-svg-icons";

const Footer = () => {
  return (
    <>
      <footer className="footer has-background-white">
        <div className="content has-text-left">
          <div className="container mb-6">
            {/* <div className="links">
              <div className="columns">
                <div className="column">
                  <div className="item has-text-weight-bold">
                    <Link to="/why-spring">Why Spring</Link>
                  </div>
                  <div className="item">
                    <Link to="/microservices">Microservices</Link>
                  </div>
                  <div className="item">
                    <Link to="/reactive">Reactive</Link>
                  </div>
                  <div className="item">
                    <Link to="/event-driven">Event Driven</Link>
                  </div>
                  <div className="item">
                    <Link to="/cloud">Cloud</Link>
                  </div>
                  <div className="item">
                    <Link to="/web-applications">Web Applications</Link>
                  </div>
                  <div className="item">
                    <Link to="/serverless">Serverless</Link>
                  </div>
                  <div className="item">
                    <Link to="/batch">Batch</Link>
                  </div>
                </div>
                <div className="column">
                  <div className="item has-text-weight-bold">
                    <Link to="/learn">Learn</Link>
                  </div>
                  <div className="item">
                    <Link to="/quickstart">Quickstart</Link>
                  </div>
                  <div className="item">
                    <Link to="/guides">Guides</Link>
                  </div>
                  <div className="item">
                    <Link to="/blog">Blog</Link>
                  </div>
                </div>
                <div className="column">
                  <div className="item has-text-weight-bold">
                    <Link to="/community">Community</Link>
                  </div>
                  <div className="item">
                    <Link to="/events">Events</Link>
                  </div>
                  <div className="item">
                    <Link to="/team">Team</Link>
                  </div>
                </div>
                <div className="column">
                  <div className="item has-text-weight-bold">
                    <Link to="/projects">Projects</Link>
                  </div>
                  <div className="item has-text-weight-bold">
                    <Link to="/training">Training</Link>
                  </div>
                  <div className="item has-text-weight-bold">
                    <Link to="/support">Support</Link>
                  </div>
                  <div className="item has-text-weight-bold">
                    <Link to="/thank-you">Thank You</Link>
                  </div>
                </div>
              </div>
            </div> */}
            <div className="container more">
              <div className="columns">
                <div className="column">
                  <p>
                    <img src={logo} alt="Spring" style={{ height: "48px" }} />
                  </p>
                  <p className="has-text-dark">
                    © 2022 VMware, Inc. or its affiliates.
                  </p>
                </div>
                <div className="social-links column is-3">
                  <a
                    href="https://github.com/spring-projects/spring-net/discussions/"
                    className="button is-black is-rounded"
                  >
                    <FontAwesomeIcon icon={faMessage} />
                  </a>
                  <a
                    href="https://github.com/spring-projects/spring-net/issues"
                    className="button is-black is-rounded"
                  >
                    <FontAwesomeIcon icon={faJira} />
                  </a>
                  <a
                    href="https://github.com/springsource/spring-net/"
                    className="button is-black is-rounded"
                  >
                    <FontAwesomeIcon icon={faGithub} />
                  </a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </footer>
    </>
  );
};

export default Footer;
