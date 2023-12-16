import { graphql, Link } from "gatsby";
import * as React from "react";
import { HTMLContent } from "../components/common/Content";
import Layout from "../components/layout/Layout";

// props
interface Props {
  data: {
    page: {
      html: any;
      frontmatter: {
        title: string;
        description?: string;
        keywords?: Array<string>;
      };
    };
  };
}

// markup
const HomepageTemplate = (props: Props) => {
  return (
    <Layout
      seo={{
        title: props.data.page.frontmatter.title,
        description: props.data.page.frontmatter.description,
        keywords: props.data.page.frontmatter.keywords,
      }}
      className="home"
    >
      <div className="default-banner">
        <div className="extra">
          <img src={`/img/extra-1.svg`} />
        </div>
      </div>
      <div className="container my-6">
        <HTMLContent
          content={props.data.page.html}
          className={"markdown content"}
        ></HTMLContent>
      </div>
    </Layout>
  );
};

export default HomepageTemplate;

// graphQL queries
export const pageQuery = graphql`
  query HomepageTemplate($id: String!) {
    page: markdownRemark(id: { eq: $id }) {
      html
      frontmatter {
        title
        description
        keywords
      }
    }
  }
`;
