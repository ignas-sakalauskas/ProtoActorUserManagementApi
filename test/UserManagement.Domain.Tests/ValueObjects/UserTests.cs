using System;
using AutoFixture.Xunit2;
using FluentAssertions;
using UserManagement.Domain.ValueObjects;
using Xunit;

namespace UserManagement.Domain.Tests.ValueObjects
{
    public class UserTests
    {
        [Theory, AutoData]
        public void Given_empty_user_id_when_creating_should_throw(string name, DateTimeOffset createdOn)
        {
            // Given
            // When
            Action action = () => new User(Guid.Empty, name, createdOn);

            // Then
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineAutoData(null)]
        [InlineAutoData("")]
        public void Given_empty_user_name_when_creating_should_throw(string invalidName, Guid id, DateTimeOffset createdOn)
        {
            // Given
            // When
            Action action = () => new User(id, invalidName, createdOn);

            // Then
            action.Should().ThrowExactly<ArgumentException>();
        }
        [Theory, AutoData]
        public void Given_invalid_created_on_date_when_creating_should_throw(string name, Guid id)
        {
            // Given
            // When
            Action action = () => new User(id, name, DateTimeOffset.MinValue);

            // Then
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Theory, AutoData]
        public void Given_valid_params_when_creating_should_assign_properties(Guid id, string name, DateTimeOffset createdOn)
        {
            // Given
            // When
            var result = new User(id, name, createdOn);

            // Then
            result.Id.Should().Be(id);
            result.Name.Should().Be(name);
        }

        [Theory, AutoData]
        public void Given_two_instances_with_the_same_properties_when_comparing_should_be_equal(Guid id, string name, DateTimeOffset createdOn)
        {
            // Given
            var user1 = new User(id, name, createdOn);
            var user2 = new User(id, name, createdOn);

            // When
            var result = user1 == user2;

            // Then
            result.Should().BeTrue();
        }

        [Theory, AutoData]
        public void Given_user_when_getting_hash_code_should_not_be_zero(Guid id, string name, DateTimeOffset createdOn)
        {
            // Given
            var user = new User(id, name, createdOn);

            // When
            var result = user.GetHashCode();

            // Then
            result.Should().NotBe(0);
        }
    }
}
