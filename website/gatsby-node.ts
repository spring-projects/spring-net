const _ = require("lodash");
const path = require("path");
const { createFilePath } = require("gatsby-source-filesystem");

exports.createPages = ({ actions, graphql }) => {
  const { createPage } = actions;
  return graphql(`
    {
      allMarkdownRemark {
        edges {
          node {
            id
            fields {
              slug
            }
            frontmatter {
              templateKey
              path
            }
          }
        }
      }
    }
  `).then((result) => {
    if (result.errors) {
      result.errors.forEach((e) => console.error(e.toString()));
      return Promise.reject(result.errors);
    }
    const posts = _.get(result, "data.allMarkdownRemark.edges");
    posts.forEach((edge) => {
      const id = _.get(edge, "node.id");
      const keyTemplate = _.get(edge, "node.frontmatter.templateKey", "");

      switch (keyTemplate) {
        case "":
        case "blog-template":
          // nothing if there is no template define
          break;
        default:
          createPage({
            path: _.get(edge, "node.frontmatter.path"),
            component: path.resolve(`src/templates/${keyTemplate}.tsx`),
            context: { id },
          });
      }
    });
  });
};

exports.onCreateNode = ({ node, actions, getNode }) => {
  const { createNodeField } = actions;
  // fmImagesToRelative(node); // convert image paths for gatsby images
  if (_.get(node, "internal.type") === `MarkdownRemark`) {
    let value = createFilePath({ node, getNode });
    createNodeField({
      name: `slug`,
      node,
      value,
    });
  }
};
